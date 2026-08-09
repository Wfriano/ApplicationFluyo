using FluyoV2.Exceptions;
using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Transactions.Dtos;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Liabilities.Models;
using FluyoV2.Features.Liabilities.Repositories;
using FluyoV2.Features.Transactions.Repositories;
using Microsoft.Extensions.Logging;
using FluyoV2.Constants;
using MongoDB.Bson;

namespace FluyoV2.Features.Transactions.Services;

public class TransactionsService
{
    private readonly TransactionsRepository _repository;
    private readonly AccountsRepository _accountsRepository;
    private readonly LiabilitiesRepository _liabilitiesRepository;
    private readonly ILogger<TransactionsService> _logger;
    private readonly RecurrencesService _recurrencesService;

    public TransactionsService(
        TransactionsRepository repository,
        AccountsRepository accountsRepository,
        LiabilitiesRepository liabilitiesRepository,
        ILogger<TransactionsService> logger,
        RecurrencesService recurrencesService)
    {
        _repository = repository;
        _accountsRepository = accountsRepository;
        _liabilitiesRepository = liabilitiesRepository;
        _logger = logger;
        _recurrencesService = recurrencesService;
    }

    public async Task<TransactionResponse> CreateIncomeAsync(
        string userId,
        CreateTransactionRequest request)
    {
        var account = await _accountsRepository
            .GetByIdAsync(request.AccountId, userId);

        if (account is null)
        {
            _logger.LogWarning(
                "Intento de registrar ingreso en cuenta inexistente. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                request.AccountId);

            throw new BusinessException("Cuenta no encontrada");
        }

        if (!request.IsPaid)
        {
            var firstDayOfSelectedMonth = FirstDayOfSelectedMonthUtc(request.TransactionDate);
            var recurrenceRequest = request.Recurrence ?? new CreateRecurrenceRequest
            {
                Frequency = "Mensual",
                NextDate = firstDayOfSelectedMonth,
                EndDate = firstDayOfSelectedMonth,
                Amount = request.Amount,
                Type = TransactionTypes.Income,
                Category = request.Category,
                Description = request.Description,
                AccountId = account.Id,
                IsPaid = false,
                Note = "Ingreso diferido por no pagado"
            };

            recurrenceRequest.TransactionId = ObjectId.GenerateNewId().ToString();
            recurrenceRequest.Type = TransactionTypes.Income;
            recurrenceRequest.Category = request.Category;
            recurrenceRequest.Description = request.Description;
            recurrenceRequest.AccountId = account.Id;

            await _recurrencesService.CreateAsync(userId, recurrenceRequest);

            await CreateLoanLiabilityIfNeededAsync(userId, request);

            _logger.LogInformation(
                "Ingreso diferido programado. UserId: {UserId}, AccountId: {AccountId}, Fecha: {Date}",
                userId,
                account.Id,
                firstDayOfSelectedMonth);

            return new TransactionResponse
            {
                Id = string.Empty,
                AccountId = account.Id,
                Category = request.Category,
                Type = TransactionTypes.Income,
                Amount = request.Amount,
                Description = request.Description,
                TransactionDate = firstDayOfSelectedMonth,
                CreatedAt = DateTime.UtcNow
            };
        }

        account.Balance += request.Amount;

        await _accountsRepository.UpdateBalanceAsync(
            account.Id,
            account.Balance);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = account.Id,
            Category = request.Category,
            Type = TransactionTypes.Income,
            Amount = request.Amount,
            Description = request.Description,
            TransactionDate = request.TransactionDate
        };

        await _repository.CreateAsync(transaction);

        if (request.Recurrence is not null)
        {
            var recReq = request.Recurrence;
            recReq.TransactionId = transaction.Id;
            recReq.Type = TransactionTypes.Income;
            recReq.Category = request.Category;
            recReq.Description = request.Description;
            recReq.AccountId = account.Id;

            await _recurrencesService.CreateAsync(userId, recReq);
        }

        await CreateLoanLiabilityIfNeededAsync(userId, request);

        _logger.LogInformation(
            "Ingreso registrado. UserId: {UserId}, AccountId: {AccountId}, Amount: {Amount}",
            userId,
            account.Id,
            request.Amount);

        return Map(transaction);
    }

    public async Task<TransactionResponse> CreateWithOptionalRecurrenceAsync(
        string userId,
        CreateTransactionWithRecurrenceRequest request)
    {
        // Validate account
        var account = await _accountsRepository
            .GetByIdAsync(request.AccountId, userId);

        if (account is null)
        {
            _logger.LogWarning(
                "Intento de registrar transaccion en cuenta inexistente. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                request.AccountId);

            throw new BusinessException("Cuenta no encontrada");
        }

        // Adjust balance depending on type
        if (string.Equals(request.Type, TransactionTypes.Income, StringComparison.OrdinalIgnoreCase))
            account.Balance += request.Amount;
        else
            account.Balance -= request.Amount;

        await _accountsRepository.UpdateBalanceAsync(
            account.Id,
            account.Balance);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = account.Id,
            Category = request.Category,
            Type = request.Type,
            Amount = request.Amount,
            Description = request.Description,
            TransactionDate = request.TransactionDate
        };

        await _repository.CreateAsync(transaction);

        // If recurrence info provided, create recurrence linked to transaction
        if (request.Recurrence is not null)
        {
            var recReq = request.Recurrence;
            // ensure TransactionId will be set
            recReq.TransactionId = transaction.Id;

            await _recurrencesService.CreateAsync(userId, recReq);
        }

        _logger.LogInformation(
            "Transaccion registrada. UserId: {UserId}, AccountId: {AccountId}, Amount: {Amount}",
            userId,
            account.Id,
            request.Amount);

        return Map(transaction);
    }

    public async Task<TransactionResponse> CreateExpenseAsync(
        string userId,
        CreateTransactionRequest request)
    {
        var account = await _accountsRepository
            .GetByIdAsync(request.AccountId, userId);

        if (account is null)
        {
            _logger.LogWarning(
                "Intento de registrar gasto en cuenta inexistente. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                request.AccountId);

            throw new BusinessException("Cuenta no encontrada");
        }

        if (!request.IsPaid)
        {
            var firstDayOfSelectedMonth = FirstDayOfSelectedMonthUtc(request.TransactionDate);
            var recurrenceRequest = request.Recurrence ?? new CreateRecurrenceRequest
            {
                Frequency = "Mensual",
                NextDate = firstDayOfSelectedMonth,
                EndDate = firstDayOfSelectedMonth,
                Amount = request.Amount,
                Type = TransactionTypes.Expense,
                Category = request.Category,
                Description = request.Description,
                AccountId = account.Id,
                IsPaid = false,
                Note = "Gasto diferido por no pagado"
            };

            recurrenceRequest.TransactionId = ObjectId.GenerateNewId().ToString();
            recurrenceRequest.Type = TransactionTypes.Expense;
            recurrenceRequest.Category = request.Category;
            recurrenceRequest.Description = request.Description;
            recurrenceRequest.AccountId = account.Id;

            await _recurrencesService.CreateAsync(userId, recurrenceRequest);

            _logger.LogInformation(
                "Gasto diferido programado como compromiso pendiente. UserId: {UserId}, AccountId: {AccountId}, Fecha: {Date}",
                userId,
                account.Id,
                firstDayOfSelectedMonth);

            return new TransactionResponse
            {
                Id = string.Empty,
                AccountId = account.Id,
                Category = request.Category,
                Type = TransactionTypes.Expense,
                Amount = request.Amount,
                Description = request.Description,
                TransactionDate = firstDayOfSelectedMonth,
                CreatedAt = DateTime.UtcNow
            };
        }

        account.Balance -= request.Amount;

        await _accountsRepository.UpdateBalanceAsync(
            account.Id,
            account.Balance);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = account.Id,
            Category = request.Category,
            Type = TransactionTypes.Expense,
            Amount = request.Amount,
            Description = request.Description,
            TransactionDate = request.TransactionDate
        };

        await _repository.CreateAsync(transaction);

        if (request.Recurrence is not null)
        {
            var recReq = request.Recurrence;
            recReq.TransactionId = transaction.Id;
            recReq.Type = TransactionTypes.Expense;
            recReq.Category = request.Category;
            recReq.Description = request.Description;
            recReq.AccountId = account.Id;

            await _recurrencesService.CreateAsync(userId, recReq);
        }

        _logger.LogInformation(
            "Gasto registrado. UserId: {UserId}, AccountId: {AccountId}, Amount: {Amount}",
            userId,
            account.Id,
            request.Amount);

        return Map(transaction);
    }

    private async Task CreateLoanLiabilityIfNeededAsync(
        string userId,
        CreateTransactionRequest request)
    {
        if (!IsLoanCategory(request.Category))
            return;

        if (!request.LoanPaymentDay.HasValue
            || !request.LoanInstallments.HasValue
            || !request.LoanInstallmentAmount.HasValue)
            return;

        var liability = new Liability
        {
            UserId = userId,
            Name = string.IsNullOrWhiteSpace(request.Description)
                ? "Préstamo"
                : request.Description,
            TotalAmount = request.Amount,
            IsStillPaying = true,
            PaymentFrequency = "Mensual",
            InstallmentAmount = request.LoanInstallmentAmount.Value,
            RemainingInstallments = request.LoanInstallments.Value,
            NextPaymentDate = BuildNextPaymentDateUtc(request.LoanPaymentDay.Value)
        };

        await _liabilitiesRepository.CreateAsync(liability);
    }

    private static bool IsLoanCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return false;

        return category.Equals("Préstamo", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Prestamo", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Prestamos", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Préstamos", StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime BuildNextPaymentDateUtc(int paymentDay)
    {
        var today = DateTime.UtcNow;
        var day = Math.Clamp(paymentDay, 1, 31);

        var daysInCurrentMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var currentMonthDay = Math.Min(day, daysInCurrentMonth);
        var candidate = new DateTime(today.Year, today.Month, currentMonthDay, 0, 0, 0, DateTimeKind.Utc);

        if (candidate.Date >= today.Date)
            return candidate;

        var nextMonth = today.AddMonths(1);
        var daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        var nextMonthDay = Math.Min(day, daysInNextMonth);
        return new DateTime(nextMonth.Year, nextMonth.Month, nextMonthDay, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime FirstDayOfSelectedMonthUtc(DateTime selectedDate)
    {
        var source = selectedDate == default
            ? DateTime.UtcNow
            : selectedDate;

        return new DateTime(
            source.Year,
            source.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);
    }

    public async Task<TransactionResponse?> GetByIdAsync(string userId, string id)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction is null || transaction.UserId != userId)
            return null;

        var recurrence = await _recurrencesService.GetByTransactionIdAsync(userId, transaction.Id);

        return Map(transaction, recurrence);
    }

    public async Task UpdateAsync(string userId, UpdateTransactionRequest request)
    {
        var transaction = await _repository.GetByIdAsync(request.Id);
        if (transaction is null || transaction.UserId != userId)
            throw new ArgumentException("Transaccion no encontrada o no autorizada");

        transaction.AccountId = request.AccountId;
        transaction.Amount = request.Amount;
        transaction.Category = request.Category;
        transaction.Type = request.Type;
        transaction.Description = request.Description;
        transaction.TransactionDate = request.TransactionDate;

        await _repository.UpdateAsync(transaction);

        var existingRecurrence = await _recurrencesService.GetByTransactionIdAsync(userId, transaction.Id);

        if (request.Recurrence is not null)
        {
            if (existingRecurrence is null)
            {
                var recCreate = request.Recurrence;
                recCreate.TransactionId = transaction.Id;
                recCreate.AccountId = request.AccountId;
                recCreate.Type = request.Type;
                recCreate.Category = request.Category;
                recCreate.Description = request.Description;

                await _recurrencesService.CreateAsync(userId, recCreate);
            }
            else
            {
                await _recurrencesService.UpdateAsync(userId, new RecurrenceResponse
                {
                    Id = existingRecurrence.Id,
                    TransactionId = transaction.Id,
                    Frequency = request.Recurrence.Frequency,
                    NextDate = request.Recurrence.NextDate,
                    EndDate = request.Recurrence.EndDate,
                    CreatedAt = existingRecurrence.CreatedAt,
                    Amount = request.Recurrence.Amount,
                    Type = request.Type,
                    Category = request.Category,
                    Description = request.Description,
                    AccountId = request.AccountId,
                    OtherAccountId = request.Recurrence.OtherAccountId,
                    IsPaid = request.Recurrence.IsPaid,
                    Note = request.Recurrence.Note
                });
            }
        }
    }

    public async Task DeleteAsync(string userId, string id)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction is null || transaction.UserId != userId)
            throw new ArgumentException("Transaccion no encontrada o no autorizada");

        await _repository.DeleteAsync(id);
    }

    public async Task<List<TransactionResponse>> GetAllAsync(string userId)
    {
        var transactions = await _repository.GetByUserAsync(userId);
        var recurrences = await _recurrencesService.GetAllByUserAsync(userId);

        _logger.LogInformation(
            "Movimientos consultados. UserId: {UserId}, Total: {Total}",
            userId,
            transactions.Count);

        var recurrencesByTransactionId = recurrences
            .Where(x => !string.IsNullOrWhiteSpace(x.TransactionId))
            .GroupBy(x => x.TransactionId)
            .ToDictionary(x => x.Key, x => x.First());

        return transactions
            .Select(transaction =>
            {
                recurrencesByTransactionId.TryGetValue(transaction.Id, out var recurrence);
                return Map(transaction, recurrence);
            })
            .ToList();
    }

    private static TransactionResponse Map(
        Transaction transaction,
        RecurrenceResponse? recurrence = null)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            Category = transaction.Category,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt,
            IsRecurring = recurrence is not null,
            ScheduledDate = recurrence?.NextDate,
            Recurrence = recurrence
        };
    }
}

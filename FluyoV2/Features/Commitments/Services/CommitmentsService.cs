using FluyoV2.Constants;
using FluyoV2.Exceptions;
using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Commitments.Dtos;
using FluyoV2.Features.Commitments.Models;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Transactions.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Commitments.Services;

public class CommitmentsService
{
    private readonly CommitmentsRepository _repository;
    private readonly AccountsRepository _accountsRepository;
    private readonly TransactionsRepository _transactionsRepository;
    private readonly RecurrencesRepository _recurrencesRepository;
    private readonly ILogger<CommitmentsService> _logger;

    public CommitmentsService(
        CommitmentsRepository repository,
        AccountsRepository accountsRepository,
        TransactionsRepository transactionsRepository,
        RecurrencesRepository recurrencesRepository,
        ILogger<CommitmentsService> logger)
    {
        _repository = repository;
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
        _recurrencesRepository = recurrencesRepository;
        _logger = logger;
    }

    public async Task<CommitmentResponse> CreateAsync(
        string userId,
        CreateCommitmentRequest request)
    {
        // avoid duplicate commitments with same name, payment date and amount
        var existing = (await _repository.GetByUserAsync(userId))
            .FirstOrDefault(x => x.Name == request.Name
                && x.Amount == request.Amount
                && x.Category == request.Category
                && ((x.PaymentDate.HasValue && request.PaymentDate.HasValue && x.PaymentDate.Value.Date == request.PaymentDate.Value.Date)
                    || (!x.PaymentDate.HasValue && !request.PaymentDate.HasValue))
                && (string.IsNullOrEmpty(x.Notes) && string.IsNullOrEmpty(request.Notes) || x.Notes == request.Notes));

        if (existing is not null)
        {
            _logger.LogInformation(
                "Compromiso ya existe. UserId: {UserId}, CommitmentId: {CommitmentId}, Name: {Name}",
                userId,
                existing.Id,
                existing.Name);

            return Map(existing);
        }

        var commitment = new Commitment
        {
            UserId = userId,
            Name = request.Name,
            Category = request.Category,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            Notes = request.Notes ?? string.Empty
        };

        await _repository.CreateAsync(commitment);

        _logger.LogInformation(
            "Compromiso creado. UserId: {UserId}, CommitmentId: {CommitmentId}, Name: {Name}",
            userId,
            commitment.Id,
            commitment.Name);

        return Map(commitment);
    }

    public async Task<List<CommitmentResponse>> GetAllAsync(
        string userId)
    {
        var commitments = await _repository.GetByUserAsync(userId);

        _logger.LogInformation(
            "Compromisos consultados. UserId: {UserId}, Total: {Total}",
            userId,
            commitments.Count);

        // return only active commitments
        return commitments
            .Where(c => c.IsActive)
            .Select(Map)
            .ToList();
    }

    public async Task<List<UpcomingCommitmentResponse>> GetUpcomingAsync(
        string userId,
        int? month = null,
        int? year = null)
    {
        var commitments = await _repository.GetByUserAsync(userId);

        var now = DateTime.UtcNow;
        var targetMonth = month ?? now.Month;
        var targetYear = year ?? now.Year;

        var upcoming = commitments
            .Where(c => c.IsActive && c.PaymentDate.HasValue)
            .Select(c =>
            {
                // compute next due date for the requested month using original payment day
                var daysInMonth = DateTime.DaysInMonth(targetYear, targetMonth);
                var day = Math.Min(c.PaymentDate.Value.Day, daysInMonth);
                var dueDate = new DateTime(targetYear, targetMonth, day);

                var paidThisMonth = c.LastPaymentDate.HasValue
                    && c.LastPaymentDate.Value.Month == targetMonth
                    && c.LastPaymentDate.Value.Year == targetYear;

                return new UpcomingCommitmentResponse
                {
                    Id = c.Id,
                    AccountId = c.AccountId,
                    Name = c.Name,
                    Category = c.Category,
                    Amount = c.Amount,
                    DueDate = dueDate,
                    IsPaid = paidThisMonth
                    ,
                    Notes = c.Notes ?? string.Empty
                };
            })
            // only those not paid this month
            .Where(x => !x.IsPaid)
            .OrderBy(x => x.DueDate)
            .ToList();

        _logger.LogInformation(
            "Próximos compromisos consultados. UserId: {UserId}, Count: {Count}",
            userId,
            upcoming.Count);

        return upcoming;
    }

    public async Task<CommitmentResponse?> GetByIdAsync(
        string id,
        string userId)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
            return null;

        return Map(commitment);
    }

    public async Task<decimal> GetPendingTotalAsync(string userId)
    {
        var commitments = await _repository.GetByUserAsync(userId);

        var total = commitments
            .Where(c => c.IsActive)
            .Sum(c => c.Amount);

        _logger.LogInformation(
            "Balance pendiente calculado. UserId: {UserId}, Total: {Total}",
            userId,
            total);

        return total;
    }

    public async Task<CommitmentResponse?> UpdateAsync(
        string id,
        string userId,
        UpdateCommitmentRequest request)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
            return null;

        commitment.Name = request.Name;
        commitment.Category = request.Category;
        commitment.Amount = request.Amount;
        commitment.PaymentDate = request.PaymentDate;
        commitment.IsActive = request.IsActive;

        await _repository.UpdateAsync(commitment);

        _logger.LogInformation(
            "Compromiso actualizado. UserId: {UserId}, CommitmentId: {CommitmentId}",
            userId,
            commitment.Id);

        return Map(commitment);
    }

    public async Task<bool> DeleteAsync(
        string id,
        string userId)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
            return false;

        await _repository.DeleteAsync(id);

        _logger.LogInformation(
            "Compromiso eliminado. UserId: {UserId}, CommitmentId: {CommitmentId}",
            userId,
            id);

        return true;
    }

    public async Task<bool> DeleteRecurringSeriesAsync(
        string id,
        string userId)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
            return false;

        var recurrenceId = ExtractRecurrenceId(commitment.Notes);

        // Elimina el compromiso actual
        await _repository.DeleteAsync(id);

        if (!string.IsNullOrWhiteSpace(recurrenceId))
        {
            // Elimina otros compromisos generados por la misma recurrencia
            var userCommitments = await _repository.GetByUserAsync(userId);

            var relatedCommitments = userCommitments
                .Where(x => x.Id != id
                    && x.IsActive
                    && IsFromRecurrence(x.Notes, recurrenceId))
                .ToList();

            foreach (var item in relatedCommitments)
            {
                await _repository.DeleteAsync(item.Id);
            }

            // Elimina la configuración de recurrencia para que no vuelva a generarse a futuro
            await _recurrencesRepository.DeleteAsync(recurrenceId);
        }

        _logger.LogInformation(
            "Serie recurrente eliminada. UserId: {UserId}, CommitmentId: {CommitmentId}, RecurrenceId: {RecurrenceId}",
            userId,
            id,
            recurrenceId ?? string.Empty);

        return true;
    }

    public async Task<CommitmentResponse?> PayCommitmentAsync(
        string id,
        string userId,
        FluyoV2.Features.Commitments.Dtos.PayCommitmentRequest? request)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
        {
            _logger.LogWarning(
                "Compromiso no encontrado. UserId: {UserId}, CommitmentId: {CommitmentId}",
                userId,
                id);

            return null;
        }

        var accountIdToUse = request?.AccountId;
        if (string.IsNullOrEmpty(accountIdToUse))
            accountIdToUse = commitment.AccountId;

        if (string.IsNullOrEmpty(accountIdToUse))
        {
            _logger.LogWarning(
                "No se especificó cuenta para pagar el compromiso. UserId: {UserId}, CommitmentId: {CommitmentId}",
                userId,
                id);

            throw new BusinessException(
                "Debe seleccionar una cuenta para pagar el compromiso");
        }

        var account = await _accountsRepository.GetByIdAsync(
            accountIdToUse,
            userId);

        if (account is null)
        {
            _logger.LogWarning(
                "Cuenta no encontrada para compromiso. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                commitment.AccountId);

            return null;
        }

        // determine payment date (use provided one or now)
        var paymentDate = request?.PaymentDate ?? DateTime.UtcNow;

        if (commitment.LastPaymentDate.HasValue)
        {
            var lastPayment = commitment.LastPaymentDate.Value;

            if (lastPayment.Month == paymentDate.Month &&
                lastPayment.Year == paymentDate.Year)
            {
                _logger.LogWarning(
                    "Compromiso ya pagado este mes. UserId: {UserId}, CommitmentId: {CommitmentId}",
                    userId,
                    commitment.Id);

                throw new BusinessException(
                    "Este compromiso ya fue pagado este mes");
            }
        }

        if (account.Balance < commitment.Amount)
        {
            _logger.LogWarning(
                "Saldo insuficiente para pagar compromiso. UserId: {UserId}, AccountId: {AccountId}, Balance: {Balance}, Amount: {Amount}",
                userId,
                account.Id,
                account.Balance,
                commitment.Amount);

            throw new BusinessException(
                "Saldo insuficiente para pagar el compromiso");
        }

        account.Balance -= commitment.Amount;

        await _accountsRepository.UpdateBalanceAsync(
            account.Id,
            account.Balance);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = account.Id,
            Category = commitment.Category,
            Type = TransactionTypes.Expense,
            Amount = commitment.Amount,
            Description = $"Pago automático: {commitment.Name}",
            TransactionDate = paymentDate
        };

        await _transactionsRepository.CreateAsync(transaction);

        commitment.LastPaymentDate = paymentDate;

        // after creating the transaction, remove the commitment from pending list
        await _repository.DeleteAsync(commitment.Id);

        _logger.LogInformation(
            "Compromiso pagado y eliminado. UserId: {UserId}, CommitmentId: {CommitmentId}, Amount: {Amount}",
            userId,
            commitment.Id,
            commitment.Amount);

        // return the commitment info that was paid
        return Map(commitment);
    }

    private static bool IsFromRecurrence(string? notes, string recurrenceId)
    {
        return ExtractRecurrenceId(notes) == recurrenceId;
    }

    private static string? ExtractRecurrenceId(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;

        // Expected format: Recurrence:{recurrenceId}:{yyyy-MM}
        var parts = notes.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            return null;

        if (!parts[0].Equals("Recurrence", StringComparison.OrdinalIgnoreCase))
            return null;

        return parts[1];
    }

    private static CommitmentResponse Map(
        Commitment commitment)
    {
        return new CommitmentResponse
        {
            Id = commitment.Id,
            AccountId = commitment.AccountId,
            Name = commitment.Name,
            Category = commitment.Category,
            Amount = commitment.Amount,
            PaymentDate = commitment.PaymentDate,
            IsActive = commitment.IsActive,
            Notes = commitment.Notes ?? string.Empty,
            LastPaymentDate = commitment.LastPaymentDate,
            CreatedAt = commitment.CreatedAt
        };
    }
}

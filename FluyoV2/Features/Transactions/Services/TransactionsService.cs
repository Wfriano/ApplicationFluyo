using FluyoV2.Exceptions;
using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Transactions.Dtos;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Transactions.Repositories;
using Microsoft.Extensions.Logging;
using FluyoV2.Constants;

namespace FluyoV2.Features.Transactions.Services;

public class TransactionsService
{
    private readonly TransactionsRepository _repository;
    private readonly AccountsRepository _accountsRepository;
    private readonly ILogger<TransactionsService> _logger;
    private readonly RecurrencesService _recurrencesService;

    public TransactionsService(
        TransactionsRepository repository,
        AccountsRepository accountsRepository,
        ILogger<TransactionsService> logger,
        RecurrencesService recurrencesService)
    {
        _repository = repository;
        _accountsRepository = accountsRepository;
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

        _logger.LogInformation(
            "Gasto registrado. UserId: {UserId}, AccountId: {AccountId}, Amount: {Amount}",
            userId,
            account.Id,
            request.Amount);

        return Map(transaction);
    }

    public async Task<TransactionResponse?> GetByIdAsync(string userId, string id)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction is null || transaction.UserId != userId)
            return null;

        return Map(transaction);
    }

    public async Task UpdateAsync(string userId, TransactionResponse request)
    {
        var transaction = await _repository.GetByIdAsync(request.Id);
        if (transaction is null || transaction.UserId != userId)
            throw new ArgumentException("Transaccion no encontrada o no autorizada");

        transaction.Amount = request.Amount;
        transaction.Category = request.Category;
        transaction.Description = request.Description;
        transaction.TransactionDate = request.TransactionDate;

        await _repository.UpdateAsync(transaction);
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

        _logger.LogInformation(
            "Movimientos consultados. UserId: {UserId}, Total: {Total}",
            userId,
            transactions.Count);

        return transactions.Select(Map).ToList();
    }

    private static TransactionResponse Map(Transaction transaction)
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
            CreatedAt = transaction.CreatedAt
        };
    }
}

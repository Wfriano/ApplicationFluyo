using FluyoV2.Constants;
using FluyoV2.Exceptions;
using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Transactions.Repositories;
using FluyoV2.Features.Transfers.Dtos;
using FluyoV2.Features.Transfers.Models;
using FluyoV2.Features.Transfers.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Transfers.Services;

public class TransfersService
{
    private readonly TransfersRepository _repository;
    private readonly AccountsRepository _accountsRepository;
    private readonly TransactionsRepository _transactionsRepository;
    private readonly ILogger<TransfersService> _logger;

    public TransfersService(
        TransfersRepository repository,
        AccountsRepository accountsRepository,
        TransactionsRepository transactionsRepository,
        ILogger<TransfersService> logger)
    {
        _repository = repository;
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
        _logger = logger;
    }

    public async Task<TransferResponse> CreateAsync(
        string userId,
        CreateTransferRequest request)
    {
        if (request.FromAccountId == request.ToAccountId)
            throw new BusinessException(
                "La cuenta origen y destino no pueden ser la misma");

        var fromAccount =
            await _accountsRepository.GetByIdAsync(
                request.FromAccountId,
                userId);

        if (fromAccount is null)
            throw new BusinessException(
                "Cuenta origen no encontrada");

        var toAccount =
            await _accountsRepository.GetByIdAsync(
                request.ToAccountId,
                userId);

        if (toAccount is null)
            throw new BusinessException(
                "Cuenta destino no encontrada");

        if (fromAccount.Balance < request.Amount)
            throw new BusinessException(
                "Saldo insuficiente para realizar la transferencia");

        fromAccount.Balance -= request.Amount;
        toAccount.Balance += request.Amount;

        await _accountsRepository.UpdateBalanceAsync(
            fromAccount.Id,
            fromAccount.Balance);

        await _accountsRepository.UpdateBalanceAsync(
            toAccount.Id,
            toAccount.Balance);

        var transfer = new Transfer
        {
            UserId = userId,
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            Amount = request.Amount,
            Description = request.Description
        };

        await _repository.CreateAsync(transfer);

        var expenseTransaction = new Transaction
        {
            UserId = userId,
            AccountId = fromAccount.Id,
            Category = "Transferencia",
            Type = TransactionTypes.Expense,
            Amount = request.Amount,
            Description =
                $"Transferencia a {toAccount.Name}"
        };

        await _transactionsRepository.CreateAsync(
            expenseTransaction);

        var incomeTransaction = new Transaction
        {
            UserId = userId,
            AccountId = toAccount.Id,
            Category = "Transferencia",
            Type = TransactionTypes.Income,
            Amount = request.Amount,
            Description =
                $"Transferencia desde {fromAccount.Name}"
        };

        await _transactionsRepository.CreateAsync(
            incomeTransaction);

        _logger.LogInformation(
            "Transferencia realizada. UserId: {UserId}, FromAccount: {FromAccount}, ToAccount: {ToAccount}, Amount: {Amount}",
            userId,
            fromAccount.Id,
            toAccount.Id,
            request.Amount);

        return Map(transfer);
    }

    public async Task<List<TransferResponse>> GetAllAsync(
        string userId)
    {
        var transfers =
            await _repository.GetByUserAsync(userId);

        return transfers
            .Select(Map)
            .ToList();
    }

    private static TransferResponse Map(
        Transfer transfer)
    {
        return new TransferResponse
        {
            Id = transfer.Id,
            FromAccountId = transfer.FromAccountId,
            ToAccountId = transfer.ToAccountId,
            Amount = transfer.Amount,
            Description = transfer.Description,
            CreatedAt = transfer.CreatedAt
        };
    }
}
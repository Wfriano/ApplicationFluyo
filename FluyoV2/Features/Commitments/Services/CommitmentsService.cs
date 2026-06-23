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
    private readonly ILogger<CommitmentsService> _logger;

    public CommitmentsService(
        CommitmentsRepository repository,
        AccountsRepository accountsRepository,
        TransactionsRepository transactionsRepository,
        ILogger<CommitmentsService> logger)
    {
        _repository = repository;
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
        _logger = logger;
    }

    public async Task<CommitmentResponse> CreateAsync(
        string userId,
        CreateCommitmentRequest request)
    {
        var commitment = new Commitment
        {
            UserId = userId,
            AccountId = request.AccountId,
            Name = request.Name,
            Category = request.Category,
            Amount = request.Amount,
            DayOfMonth = request.DayOfMonth
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

        return commitments.Select(Map).ToList();
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
        commitment.DayOfMonth = request.DayOfMonth;
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

    public async Task<CommitmentResponse?> PayCommitmentAsync(
        string id,
        string userId)
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

        var account = await _accountsRepository.GetByIdAsync(
            commitment.AccountId,
            userId);

        if (account is null)
        {
            _logger.LogWarning(
                "Cuenta no encontrada para compromiso. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                commitment.AccountId);

            return null;
        }

        if (commitment.LastPaymentDate.HasValue)
        {
            var lastPayment = commitment.LastPaymentDate.Value;
            var currentDate = DateTime.UtcNow;

            if (lastPayment.Month == currentDate.Month &&
                lastPayment.Year == currentDate.Year)
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
            Description = $"Pago automático: {commitment.Name}"
        };

        await _transactionsRepository.CreateAsync(transaction);

        commitment.LastPaymentDate = DateTime.UtcNow;

        await _repository.UpdateAsync(commitment);

        _logger.LogInformation(
            "Compromiso pagado. UserId: {UserId}, CommitmentId: {CommitmentId}, Amount: {Amount}",
            userId,
            commitment.Id,
            commitment.Amount);

        return Map(commitment);
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
            DayOfMonth = commitment.DayOfMonth,
            IsActive = commitment.IsActive,
            LastPaymentDate = commitment.LastPaymentDate,
            CreatedAt = commitment.CreatedAt
        };
    }
}
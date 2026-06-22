using FluyoV2.Features.Commitments.Dtos;
using FluyoV2.Features.Commitments.Models;
using FluyoV2.Features.Commitments.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Commitments.Services;

public class CommitmentsService
{
    private readonly CommitmentsRepository _repository;
    private readonly ILogger<CommitmentsService> _logger;

    public CommitmentsService(
        CommitmentsRepository repository,
        ILogger<CommitmentsService> logger)
    {
        _repository = repository;
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
        var commitments =
            await _repository.GetByUserAsync(userId);

        _logger.LogInformation(
            "Compromisos consultados. UserId: {UserId}, Total: {Total}",
            userId,
            commitments.Count);

        return commitments
            .Select(Map)
            .ToList();
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
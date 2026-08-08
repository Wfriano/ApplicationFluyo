using FluyoV2.Features.Commitments.Models;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Liabilities.Dtos;
using FluyoV2.Features.Liabilities.Models;
using FluyoV2.Features.Liabilities.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Liabilities.Services;

public class LiabilitiesService
{
    private readonly LiabilitiesRepository _repository;
    private readonly CommitmentsRepository _commitmentsRepository;
    private readonly ILogger<LiabilitiesService> _logger;

    public LiabilitiesService(
        LiabilitiesRepository repository,
        CommitmentsRepository commitmentsRepository,
        ILogger<LiabilitiesService> logger)
    {
        _repository = repository;
        _commitmentsRepository = commitmentsRepository;
        _logger = logger;
    }

    public async Task<LiabilityResponse> CreateAsync(
        string userId,
        CreateLiabilityRequest request)
    {
        var liability = new Liability
        {
            UserId = userId,
            Name = request.Name,
            TotalAmount = request.TotalAmount,
            IsStillPaying = request.IsStillPaying,
            PaymentFrequency = request.PaymentFrequency,
            InstallmentAmount = request.InstallmentAmount,
            RemainingInstallments = request.RemainingInstallments,
            NextPaymentDate = request.NextPaymentDate
        };

        await _repository.CreateAsync(liability);

        await CreatePendingCommitmentIfNeededAsync(
            userId,
            liability.Name,
            liability.IsStillPaying,
            liability.PaymentFrequency,
            liability.InstallmentAmount ?? liability.TotalAmount,
            "Liability");

        _logger.LogInformation(
            "Pasivo creado. UserId: {UserId}, LiabilityId: {LiabilityId}, Name: {Name}",
            userId,
            liability.Id,
            liability.Name);

        return Map(liability);
    }

    public async Task<List<LiabilityResponse>> GetAllAsync(
        string userId)
    {
        var liabilities = await _repository.GetByUserAsync(userId);

        _logger.LogInformation(
            "Pasivos consultados. UserId: {UserId}, Total: {Total}",
            userId,
            liabilities.Count);

        return liabilities
            .Select(Map)
            .ToList();
    }

    public async Task<LiabilityResponse?> GetByIdAsync(
        string id,
        string userId)
    {
        var liability = await _repository.GetByIdAsync(id);

        if (liability is null || liability.UserId != userId || !liability.IsActive)
            return null;

        return Map(liability);
    }

    public async Task<decimal> GetTotalAmountAsync(string userId)
    {
        var liabilities = await _repository.GetByUserAsync(userId);

        var total = liabilities
            .Where(l => l.IsActive)
            .Sum(l => l.TotalAmount);

        _logger.LogInformation(
            "Monto total de pasivos calculado. UserId: {UserId}, Total: {Total}",
            userId,
            total);

        return total;
    }

    public async Task<LiabilityResponse?> UpdateAsync(
        string id,
        string userId,
        UpdateLiabilityRequest request)
    {
        var liability = await _repository.GetByIdAsync(id);

        if (liability is null || liability.UserId != userId || !liability.IsActive)
            return null;

        liability.Name = request.Name;
        liability.TotalAmount = request.TotalAmount;
        liability.IsStillPaying = request.IsStillPaying;
        liability.PaymentFrequency = request.PaymentFrequency;
        liability.InstallmentAmount = request.InstallmentAmount;
        liability.RemainingInstallments = request.RemainingInstallments;
        liability.NextPaymentDate = request.NextPaymentDate;

        await _repository.UpdateAsync(liability);

        await CreatePendingCommitmentIfNeededAsync(
            userId,
            liability.Name,
            liability.IsStillPaying,
            liability.PaymentFrequency,
            liability.InstallmentAmount ?? liability.TotalAmount,
            "Liability");

        _logger.LogInformation(
            "Pasivo actualizado. UserId: {UserId}, LiabilityId: {LiabilityId}",
            userId,
            liability.Id);

        return Map(liability);
    }

    public async Task<bool> DeleteAsync(
        string id,
        string userId)
    {
        var liability = await _repository.GetByIdAsync(id);

        if (liability is null || liability.UserId != userId || !liability.IsActive)
            return false;

        await _repository.DeleteAsync(id);

        _logger.LogInformation(
            "Pasivo eliminado. UserId: {UserId}, LiabilityId: {LiabilityId}",
            userId,
            id);

        return true;
    }

    private async Task CreatePendingCommitmentIfNeededAsync(
        string userId,
        string name,
        bool isStillPaying,
        string? paymentFrequency,
        decimal amount,
        string source)
    {
        if (!isStillPaying || string.IsNullOrWhiteSpace(paymentFrequency) || amount <= 0)
            return;

        var firstDayOfNextMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc).AddMonths(1);

        var noteMarker = $"{source}:{name}:{firstDayOfNextMonth:yyyy-MM}";

        var existing = (await _commitmentsRepository.GetByUserAsync(userId))
            .FirstOrDefault(x => x.Notes == noteMarker && x.IsActive);

        if (existing is not null)
            return;

        var commitment = new Commitment
        {
            UserId = userId,
            Name = $"Cuota {name}",
            Category = "Compromiso pendiente",
            Amount = amount,
            PaymentDate = firstDayOfNextMonth,
            Notes = noteMarker
        };

        await _commitmentsRepository.CreateAsync(commitment);
    }

    private static LiabilityResponse Map(
        Liability liability)
    {
        return new LiabilityResponse
        {
            Id = liability.Id,
            Name = liability.Name,
            TotalAmount = liability.TotalAmount,
            IsStillPaying = liability.IsStillPaying,
            PaymentFrequency = liability.PaymentFrequency,
            InstallmentAmount = liability.InstallmentAmount,
            RemainingInstallments = liability.RemainingInstallments,
            NextPaymentDate = liability.NextPaymentDate,
            CreatedAt = liability.CreatedAt,
            UpdatedAt = liability.UpdatedAt
        };
    }
}

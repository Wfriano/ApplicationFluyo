using FluyoV2.Features.Assets.Dtos;
using FluyoV2.Features.Assets.Models;
using FluyoV2.Features.Assets.Repositories;
using FluyoV2.Features.Commitments.Models;
using FluyoV2.Features.Commitments.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Assets.Services;

public class AssetsService
{
    private readonly AssetsRepository _repository;
    private readonly CommitmentsRepository _commitmentsRepository;
    private readonly ILogger<AssetsService> _logger;

    public AssetsService(
        AssetsRepository repository,
        CommitmentsRepository commitmentsRepository,
        ILogger<AssetsService> logger)
    {
        _repository = repository;
        _commitmentsRepository = commitmentsRepository;
        _logger = logger;
    }

    public async Task<AssetResponse> CreateAsync(
        string userId,
        CreateAssetRequest request)
    {
        var asset = new Asset
        {
            UserId = userId,
            Name = request.Name,
            Value = request.Value,
            IsStillPaying = request.IsStillPaying,
            PaymentFrequency = request.PaymentFrequency,
            InstallmentAmount = request.InstallmentAmount,
            RemainingInstallments = request.RemainingInstallments,
            NextPaymentDate = request.NextPaymentDate
        };

        await _repository.CreateAsync(asset);

        await CreatePendingCommitmentIfNeededAsync(
            userId,
            asset.Name,
            asset.IsStillPaying,
            asset.PaymentFrequency,
            asset.InstallmentAmount ?? asset.Value,
            "Asset");

        _logger.LogInformation(
            "Activo creado. UserId: {UserId}, AssetId: {AssetId}, Name: {Name}",
            userId,
            asset.Id,
            asset.Name);

        return Map(asset);
    }

    public async Task<List<AssetResponse>> GetAllAsync(
        string userId)
    {
        var assets = await _repository.GetByUserAsync(userId);

        _logger.LogInformation(
            "Activos consultados. UserId: {UserId}, Total: {Total}",
            userId,
            assets.Count);

        return assets
            .Select(Map)
            .ToList();
    }

    public async Task<AssetResponse?> GetByIdAsync(
        string id,
        string userId)
    {
        var asset = await _repository.GetByIdAsync(id);

        if (asset is null || asset.UserId != userId || !asset.IsActive)
            return null;

        return Map(asset);
    }

    public async Task<decimal> GetTotalValueAsync(string userId)
    {
        var assets = await _repository.GetByUserAsync(userId);

        var total = assets
            .Where(a => a.IsActive)
            .Sum(a => a.Value);

        _logger.LogInformation(
            "Valor total de activos calculado. UserId: {UserId}, Total: {Total}",
            userId,
            total);

        return total;
    }

    public async Task<AssetResponse?> UpdateAsync(
        string id,
        string userId,
        UpdateAssetRequest request)
    {
        var asset = await _repository.GetByIdAsync(id);

        if (asset is null || asset.UserId != userId || !asset.IsActive)
            return null;

        asset.Name = request.Name;
        asset.Value = request.Value;
        asset.IsStillPaying = request.IsStillPaying;
        asset.PaymentFrequency = request.PaymentFrequency;
        asset.InstallmentAmount = request.InstallmentAmount;
        asset.RemainingInstallments = request.RemainingInstallments;
        asset.NextPaymentDate = request.NextPaymentDate;

        await _repository.UpdateAsync(asset);

        await CreatePendingCommitmentIfNeededAsync(
            userId,
            asset.Name,
            asset.IsStillPaying,
            asset.PaymentFrequency,
            asset.InstallmentAmount ?? asset.Value,
            "Asset");

        _logger.LogInformation(
            "Activo actualizado. UserId: {UserId}, AssetId: {AssetId}",
            userId,
            asset.Id);

        return Map(asset);
    }

    public async Task<bool> DeleteAsync(
        string id,
        string userId)
    {
        var asset = await _repository.GetByIdAsync(id);

        if (asset is null || asset.UserId != userId || !asset.IsActive)
            return false;

        await _repository.DeleteAsync(id);

        _logger.LogInformation(
            "Activo eliminado. UserId: {UserId}, AssetId: {AssetId}",
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

    private static AssetResponse Map(
        Asset asset)
    {
        return new AssetResponse
        {
            Id = asset.Id,
            Name = asset.Name,
            Value = asset.Value,
            IsStillPaying = asset.IsStillPaying,
            PaymentFrequency = asset.PaymentFrequency,
            InstallmentAmount = asset.InstallmentAmount,
            RemainingInstallments = asset.RemainingInstallments,
            NextPaymentDate = asset.NextPaymentDate,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
    }
}

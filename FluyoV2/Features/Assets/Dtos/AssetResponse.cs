namespace FluyoV2.Features.Assets.Dtos;

public class AssetResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public bool IsStillPaying { get; set; }
    public string? PaymentFrequency { get; set; }
    public decimal? InstallmentAmount { get; set; }
    public int? RemainingInstallments { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

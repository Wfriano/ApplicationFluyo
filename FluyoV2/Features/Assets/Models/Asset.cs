namespace FluyoV2.Features.Assets.Models;

public class Asset
{
    public string Id { get; set; }
        = Guid.NewGuid().ToString();

    public string UserId { get; set; }
        = string.Empty;

    public string Name { get; set; }
        = string.Empty;

    public decimal Value { get; set; }

    public bool IsStillPaying { get; set; }

    // Payment details (only relevant if IsStillPaying is true)
    public string? PaymentFrequency { get; set; }

    public decimal? InstallmentAmount { get; set; }

    public int? RemainingInstallments { get; set; }

    public DateTime? NextPaymentDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

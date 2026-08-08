namespace FluyoV2.Features.Liabilities.Dtos;

public class CreateLiabilityRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsStillPaying { get; set; }
    public string? PaymentFrequency { get; set; }
    public decimal? InstallmentAmount { get; set; }
    public int? RemainingInstallments { get; set; }
    public DateTime? NextPaymentDate { get; set; }
}

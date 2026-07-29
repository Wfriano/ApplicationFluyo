namespace FluyoV2.Features.Commitments.Dtos;

public class CommitmentResponse
{
    public string Id { get; set; }
        = string.Empty;

    public string AccountId { get; set; }
        = string.Empty;

    public string Name { get; set; }
        = string.Empty;

    public string Category { get; set; }
        = string.Empty;

    public decimal Amount { get; set; }

    // Exact payment date (fecha de pago). Nullable if not set.
    public DateTime? PaymentDate { get; set; }

    // Optional notes
    public string Notes { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime? LastPaymentDate { get; set; }

    public DateTime CreatedAt { get; set; }
}
namespace FluyoV2.Features.Commitments.Dtos;

public class UpdateCommitmentRequest
{
    public string Name { get; set; }
        = string.Empty;

    public string Category { get; set; }
        = string.Empty;

    public decimal Amount { get; set; }

    // Optional exact payment date (fecha de pago). Nullable if user doesn't select a date.
    public DateTime? PaymentDate { get; set; }

    // Optional notes (notas)
    public string? Notes { get; set; }

    public bool IsActive { get; set; }
}
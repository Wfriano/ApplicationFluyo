using FluyoV2.Features.Transactions.Dtos;

namespace FluyoV2.Features.Commitments.Dtos;

public class CreateCommitmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }

    // Optional recurrence settings: when provided, a recurrence will be created that generates pending commitments
    public CreateRecurrenceRequest? Recurrence { get; set; }
}

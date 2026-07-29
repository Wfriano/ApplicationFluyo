namespace FluyoV2.Features.Commitments.Dtos;

public class UpcomingCommitmentResponse
{
    public string Id { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime DueDate { get; set; }

    public bool IsPaid { get; set; }

    // Optional notes
    public string Notes { get; set; } = string.Empty;
}

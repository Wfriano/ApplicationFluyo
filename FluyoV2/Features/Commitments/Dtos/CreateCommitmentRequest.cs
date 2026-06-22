namespace FluyoV2.Features.Commitments.Dtos;

public class CreateCommitmentRequest
{
    public string AccountId { get; set; }
        = string.Empty;

    public string Name { get; set; }
        = string.Empty;

    public string Category { get; set; }
        = string.Empty;

    public decimal Amount { get; set; }

    public int DayOfMonth { get; set; }
}
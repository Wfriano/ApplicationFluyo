namespace FluyoV2.Features.Commitments.Models;

public class Commitment
{
    public string Id { get; set; }
        = Guid.NewGuid().ToString();

    public string UserId { get; set; }
        = string.Empty;

    public string AccountId { get; set; }
        = string.Empty;

    public string Name { get; set; }
        = string.Empty;

    public string Category { get; set; }
        = string.Empty;

    public decimal Amount { get; set; }

    public int DayOfMonth { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastPaymentDate { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}
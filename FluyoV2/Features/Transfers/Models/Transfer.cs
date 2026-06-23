namespace FluyoV2.Features.Transfers.Models;

public class Transfer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    public string FromAccountId { get; set; } = string.Empty;

    public string ToAccountId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
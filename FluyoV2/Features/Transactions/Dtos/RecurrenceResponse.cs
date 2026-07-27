namespace FluyoV2.Features.Transactions.Dtos;

public class RecurrenceResponse
{
    public string Id { get; set; } = string.Empty;

    public string TransactionId { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public DateTime NextDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string? OtherAccountId { get; set; }

    public bool IsPaid { get; set; }

    public string? Note { get; set; }
}

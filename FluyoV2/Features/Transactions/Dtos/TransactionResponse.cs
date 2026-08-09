namespace FluyoV2.Features.Transactions.Dtos;

public class TransactionResponse
{
    public string Id { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsRecurring { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public RecurrenceResponse? Recurrence { get; set; }
}

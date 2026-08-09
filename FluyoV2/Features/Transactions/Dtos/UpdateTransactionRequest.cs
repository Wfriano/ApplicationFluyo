namespace FluyoV2.Features.Transactions.Dtos;

public class UpdateTransactionRequest
{
    public string Id { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    // Optional recurrence settings. If present, creates/updates recurrence for this transaction.
    public CreateRecurrenceRequest? Recurrence { get; set; }
}

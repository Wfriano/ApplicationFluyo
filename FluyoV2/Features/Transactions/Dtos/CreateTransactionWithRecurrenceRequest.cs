namespace FluyoV2.Features.Transactions.Dtos;

public class CreateTransactionWithRecurrenceRequest
{
    // Transaction fields
    public string? AccountId { get; set; }

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    // "Income" or "Expense"
    public string Type { get; set; } = "Income";

    // Optional recurrence settings (frontend may send null)
    public CreateRecurrenceRequest? Recurrence { get; set; }
}

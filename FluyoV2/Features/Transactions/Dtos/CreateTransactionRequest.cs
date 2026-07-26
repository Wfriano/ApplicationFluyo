namespace FluyoV2.Features.Transactions.Dtos;

public class CreateTransactionRequest
{
    public string? AccountId { get; set; }

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }
}
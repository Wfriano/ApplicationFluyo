namespace FluyoV2.Features.Transfers.Dtos;

public class TransferResponse
{
    public string Id { get; set; } = string.Empty;

    public string FromAccountId { get; set; } = string.Empty;

    public string ToAccountId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    // Additional info for UI: updated balances and account display names
    public decimal FromAccountBalance { get; set; }
    public decimal ToAccountBalance { get; set; }
    public string FromAccountName { get; set; } = string.Empty;
    public string ToAccountName { get; set; } = string.Empty;
}
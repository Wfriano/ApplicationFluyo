namespace FluyoV2.Features.Transfers.Dtos;

public class TransferResponse
{
    public string Id { get; set; } = string.Empty;

    public string FromAccountId { get; set; } = string.Empty;

    public string ToAccountId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
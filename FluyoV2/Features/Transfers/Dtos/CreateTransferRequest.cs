namespace FluyoV2.Features.Transfers.Dtos;

public class CreateTransferRequest
{
    public string FromAccountId { get; set; } = string.Empty;
    public string ToAccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
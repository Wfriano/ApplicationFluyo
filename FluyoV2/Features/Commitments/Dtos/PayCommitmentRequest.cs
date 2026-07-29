namespace FluyoV2.Features.Commitments.Dtos;

public class PayCommitmentRequest
{
    // Optional: override account to use for payment. If null, the commitment's account is used.
    public string? AccountId { get; set; }

    // Optional payment date. If not provided, UTC now will be used.
    public DateTime? PaymentDate { get; set; }
}

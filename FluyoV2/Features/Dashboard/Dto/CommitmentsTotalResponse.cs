namespace FluyoV2.Features.Dashboard.Dtos;

public class CommitmentsTotalResponse
{
    public decimal PendingCommitmentsTotal { get; set; }
    public decimal AssetsPendingInstallmentsTotal { get; set; }
    public decimal TotalToShow { get; set; }
}

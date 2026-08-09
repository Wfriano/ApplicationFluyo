namespace FluyoV2.Features.Dashboard.Dtos;

public class AssetsPendingInstallmentsResponse
{
    public decimal TotalPendingInstallments { get; set; }
    public List<AssetPendingInstallmentItem> Items { get; set; } = new();
}

public class AssetPendingInstallmentItem
{
    public string AssetId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal InstallmentAmount { get; set; }
    public int RemainingInstallments { get; set; }
    public decimal PendingTotal { get; set; }
    public DateTime? NextPaymentDate { get; set; }
}

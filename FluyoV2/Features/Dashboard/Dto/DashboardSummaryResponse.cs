namespace FluyoV2.Features.Dashboard.Dtos;

public class DashboardSummaryResponse
{
    public decimal TotalBalance { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public int TotalAccounts { get; set; }

    public int TotalTransactions { get; set; }
}
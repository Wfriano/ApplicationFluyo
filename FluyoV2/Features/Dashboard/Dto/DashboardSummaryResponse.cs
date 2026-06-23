namespace FluyoV2.Features.Dashboard.Dtos;

public class DashboardSummaryResponse
{
    public decimal TotalBalance { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public int TotalAccounts { get; set; }

    public int TotalTransactions { get; set; }

    public decimal MonthlyCommitments { get; set; }

    public decimal AvailableBalance { get; set; }

    public int ActiveGoals { get; set; }
}
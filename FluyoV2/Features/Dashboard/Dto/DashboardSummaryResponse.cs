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

    // Additional fields for frontend
    // Income and expense in the current month
    public decimal IncomeThisMonth { get; set; }
    public decimal ExpensesThisMonth { get; set; }

    // Net position: incomes - expenses
    public decimal NetPosition { get; set; }

    // Total debts (expenses) excluding transfers
    public decimal DebtsTotal { get; set; }

    // Available after subtracting debts and monthly commitments
    public decimal CurrentAvailableAfterDebts { get; set; }

    // Upcoming scheduled income (from recurrences)
    public DateTime? NextIncomeDate { get; set; }
    public decimal? NextIncomeAmount { get; set; }
    public int? DaysUntilNextIncome { get; set; }

    // Amount needed until next income (NextIncomeAmount - AvailableBalance)
    public decimal? AmountUntilNextIncome { get; set; }
}
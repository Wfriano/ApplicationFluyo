using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Dashboard.Dtos;
using FluyoV2.Features.Goals.Repositories;
using FluyoV2.Features.Transactions.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Dashboard.Services;

public class DashboardService
{
    private readonly AccountsRepository _accountsRepository;
    private readonly TransactionsRepository _transactionsRepository;
    private readonly CommitmentsRepository _commitmentsRepository;
    private readonly GoalsRepository _goalsRepository;
    private readonly FluyoV2.Features.Transactions.Repositories.RecurrencesRepository _recurrencesRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        AccountsRepository accountsRepository,
        TransactionsRepository transactionsRepository,
        CommitmentsRepository commitmentsRepository,
        GoalsRepository goalsRepository,
        FluyoV2.Features.Transactions.Repositories.RecurrencesRepository recurrencesRepository,
        ILogger<DashboardService> logger)
    {
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
        _commitmentsRepository = commitmentsRepository;
        _goalsRepository = goalsRepository;
        _recurrencesRepository = recurrencesRepository;
        _logger = logger;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        string userId)
    {
        var accounts = await _accountsRepository.GetByUserIdAsync(userId);
        var commitments = await _commitmentsRepository.GetByUserAsync(userId);
        var goals = await _goalsRepository.GetByUserAsync(userId);
        var recurrences = await _recurrencesRepository.GetByUserAsync(userId);

        var allTransactions = await _transactionsRepository.GetByUserAsync(userId);

        // compute month-based totals
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var incomeThisMonth = allTransactions
            .Where(t => t.Type == "Income" && t.TransactionDate >= monthStart)
            .Sum(t => t.Amount);

        // Exclude transfers from expense calculations
        var expensesThisMonth = allTransactions
            .Where(t => t.Type == "Expense" && t.TransactionDate >= monthStart && t.Category != "Transferencia")
            .Sum(t => t.Amount);

        // Debt total should come from active commitments (pending obligations)
        var debtsTotal = commitments
            .Where(x => x.IsActive)
            .Sum(x => x.Amount);

        var totalBalance = accounts.Sum(x => x.Balance);

        var monthlyCommitments = commitments
            .Where(x => x.IsActive)
            .Sum(x => x.Amount);

        var activeGoals = goals
            .Count(x => !x.IsCompleted);

        // find next scheduled income recurrence
        var nextIncome = recurrences
            .Where(r => r.Type == "Income" && r.NextDate >= DateTime.UtcNow)
            .OrderBy(r => r.NextDate)
            .FirstOrDefault();

        var result = new DashboardSummaryResponse
        {
            TotalBalance = totalBalance,
            TotalAccounts = accounts.Count,
            TotalIncome = await _transactionsRepository.GetTotalIncomeAsync(userId),
            // Total expenses excluding transfers
            TotalExpenses = debtsTotal,
            TotalTransactions = await _transactionsRepository.GetTotalTransactionsAsync(userId),
            MonthlyCommitments = monthlyCommitments,
            AvailableBalance = totalBalance - monthlyCommitments,
            DebtsTotal = debtsTotal,
            CurrentAvailableAfterDebts = totalBalance - debtsTotal,
            ActiveGoals = activeGoals,

            IncomeThisMonth = incomeThisMonth,
            ExpensesThisMonth = expensesThisMonth,
            NetPosition = (await _transactionsRepository.GetTotalIncomeAsync(userId)) - debtsTotal,

            NextIncomeDate = nextIncome?.NextDate,
            NextIncomeAmount = nextIncome?.Amount,
            DaysUntilNextIncome = nextIncome is null ? null : (int?)( (nextIncome.NextDate.Date - DateTime.UtcNow.Date).Days ),
            AmountUntilNextIncome = nextIncome is null ? null : Math.Max(0, nextIncome.Amount - (totalBalance - monthlyCommitments - debtsTotal))
        };

        _logger.LogInformation(
            "Dashboard consultado. UserId: {UserId}, TotalBalance: {TotalBalance}, MonthlyCommitments: {MonthlyCommitments}",
            userId,
            result.TotalBalance,
            result.MonthlyCommitments);

        return result;
    }
}
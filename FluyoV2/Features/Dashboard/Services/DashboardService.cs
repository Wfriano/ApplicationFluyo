using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Assets.Repositories;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Dashboard.Dtos;
using FluyoV2.Features.Goals.Repositories;
using FluyoV2.Features.Liabilities.Repositories;
using FluyoV2.Features.Transactions.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Dashboard.Services;

public class DashboardService
{
    private readonly AccountsRepository _accountsRepository;
    private readonly TransactionsRepository _transactionsRepository;
    private readonly CommitmentsRepository _commitmentsRepository;
    private readonly GoalsRepository _goalsRepository;
    private readonly AssetsRepository _assetsRepository;
    private readonly LiabilitiesRepository _liabilitiesRepository;
    private readonly FluyoV2.Features.Transactions.Repositories.RecurrencesRepository _recurrencesRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        AccountsRepository accountsRepository,
        TransactionsRepository transactionsRepository,
        CommitmentsRepository commitmentsRepository,
        GoalsRepository goalsRepository,
        AssetsRepository assetsRepository,
        LiabilitiesRepository liabilitiesRepository,
        FluyoV2.Features.Transactions.Repositories.RecurrencesRepository recurrencesRepository,
        ILogger<DashboardService> logger)
    {
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
        _commitmentsRepository = commitmentsRepository;
        _goalsRepository = goalsRepository;
        _assetsRepository = assetsRepository;
        _liabilitiesRepository = liabilitiesRepository;
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
        var assets = await _assetsRepository.GetByUserAsync(userId);
        var liabilities = await _liabilitiesRepository.GetByUserAsync(userId);

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

        var assetsTotal = assets
            .Where(x => x.IsActive)
            .Sum(x => x.Value);

        var liabilitiesTotal = liabilities
            .Where(x => x.IsActive)
            .Sum(x => x.TotalAmount);

        // En situación actual: lo que debes viene de liabilities
        var debtsTotal = liabilitiesTotal;

        // TotalBalance se alinea con patrimonio neto para "Tu situación actual"
        var totalBalance = assetsTotal - liabilitiesTotal;

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
            AssetsTotal = assetsTotal,
            LiabilitiesTotal = liabilitiesTotal,
            NetWorth = assetsTotal - liabilitiesTotal,
            CurrentAvailableAfterDebts = totalBalance,
            ActiveGoals = activeGoals,

            IncomeThisMonth = incomeThisMonth,
            ExpensesThisMonth = expensesThisMonth,
            NetPosition = (await _transactionsRepository.GetTotalIncomeAsync(userId)) - debtsTotal,

            NextIncomeDate = nextIncome?.NextDate,
            NextIncomeAmount = nextIncome?.Amount,
            DaysUntilNextIncome = nextIncome is null ? null : (int?)((nextIncome.NextDate.Date - DateTime.UtcNow.Date).Days),
            AmountUntilNextIncome = nextIncome is null ? null : Math.Max(0, nextIncome.Amount - (totalBalance - monthlyCommitments))
        };
        // Percentage of CurrentAvailableAfterDebts relative to TotalBalance, rounded to 2 decimals. Guard against division by zero.
        result.CurrentAvailableAfterDebtsPercentage = totalBalance == 0m ? 0m : Math.Round((result.CurrentAvailableAfterDebts / totalBalance) * 100m, 2);

        _logger.LogInformation(
            "Dashboard consultado. UserId: {UserId}, TotalBalance: {TotalBalance}, MonthlyCommitments: {MonthlyCommitments}",
            userId,
            result.TotalBalance,
            result.MonthlyCommitments);

        return result;
    }
}
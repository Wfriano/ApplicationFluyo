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

    public async Task<decimal> GetAssetsPendingInstallmentsTotalAsync(string userId)
    {
        var assets = await _assetsRepository.GetByUserAsync(userId);

        return assets
            .Where(x => x.IsActive
                && x.IsStillPaying
                && x.RemainingInstallments.HasValue
                && x.InstallmentAmount.HasValue)
            .Sum(x => x.RemainingInstallments!.Value * x.InstallmentAmount!.Value);
    }

    public async Task<CommitmentsTotalResponse> GetCommitmentsTotalAsync(string userId)
    {
        var commitments = await _commitmentsRepository.GetByUserAsync(userId);

        var pendingCommitmentsTotal = commitments
            .Where(x => x.IsActive)
            .Sum(x => x.Amount);

        var assetsPendingInstallmentsTotal = await GetAssetsPendingInstallmentsTotalAsync(userId);

        return new CommitmentsTotalResponse
        {
            PendingCommitmentsTotal = pendingCommitmentsTotal,
            AssetsPendingInstallmentsTotal = assetsPendingInstallmentsTotal,
            TotalToShow = pendingCommitmentsTotal + assetsPendingInstallmentsTotal
        };
    }

    public async Task<AssetsPendingInstallmentsResponse> GetAssetsPendingInstallmentsAsync(string userId)
    {
        var assets = await _assetsRepository.GetByUserAsync(userId);

        var items = assets
            .Where(x => x.IsActive
                && x.IsStillPaying
                && x.RemainingInstallments.HasValue
                && x.InstallmentAmount.HasValue)
            .Select(x => new AssetPendingInstallmentItem
            {
                AssetId = x.Id,
                Name = x.Name,
                InstallmentAmount = x.InstallmentAmount!.Value,
                RemainingInstallments = x.RemainingInstallments!.Value,
                PendingTotal = x.InstallmentAmount.Value * x.RemainingInstallments.Value,
                NextPaymentDate = x.NextPaymentDate
            })
            .OrderByDescending(x => x.PendingTotal)
            .ToList();

        return new AssetsPendingInstallmentsResponse
        {
            TotalPendingInstallments = items.Sum(x => x.PendingTotal),
            Items = items
        };
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

        // Deudas patrimoniales desde assets financiados:
        // suma de (cuotas restantes * valor cuota)
        var debtsTotal = assets
            .Where(x => x.IsActive
                && x.IsStillPaying
                && x.RemainingInstallments.HasValue
                && x.InstallmentAmount.HasValue)
            .Sum(x => x.RemainingInstallments!.Value * x.InstallmentAmount!.Value);

        // Compromisos pendientes activos
        var pendingCommitments = commitments
            .Where(x => x.IsActive)
            .Sum(x => x.Amount);

        var commitmentsTotals = await GetCommitmentsTotalAsync(userId);

        // TotalBalance = suma de todas las cuentas
        var totalBalance = accounts.Sum(x => x.Balance);

        var monthlyCommitments = pendingCommitments;

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
            // TotalExpenses = suma de compromisos pendientes + cuotas pendientes de assets
            TotalExpenses = commitmentsTotals.TotalToShow,
            TotalTransactions = await _transactionsRepository.GetTotalTransactionsAsync(userId),
            MonthlyCommitments = monthlyCommitments,
            AvailableBalance = totalBalance - monthlyCommitments,
            DebtsTotal = debtsTotal,
            AssetsTotal = assetsTotal,
            LiabilitiesTotal = liabilitiesTotal,
            NetWorth = assetsTotal - debtsTotal,
            CurrentAvailableAfterDebts = totalBalance - pendingCommitments,
            ActiveGoals = activeGoals,

            IncomeThisMonth = incomeThisMonth,
            ExpensesThisMonth = expensesThisMonth,
            NetPosition = (await _transactionsRepository.GetTotalIncomeAsync(userId)) - debtsTotal,

            NextIncomeDate = nextIncome?.NextDate,
            NextIncomeAmount = nextIncome?.Amount,
            DaysUntilNextIncome = nextIncome is null ? null : (int?)((nextIncome.NextDate.Date - DateTime.UtcNow.Date).Days),
            AmountUntilNextIncome = nextIncome is null ? null : Math.Max(0, nextIncome.Amount - (totalBalance - pendingCommitments))
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
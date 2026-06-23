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
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        AccountsRepository accountsRepository,
        TransactionsRepository transactionsRepository,
        CommitmentsRepository commitmentsRepository,
        GoalsRepository goalsRepository,
        ILogger<DashboardService> logger)
    {
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
        _commitmentsRepository = commitmentsRepository;
        _goalsRepository = goalsRepository;
        _logger = logger;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        string userId)
    {
        var accounts = await _accountsRepository.GetByUserIdAsync(userId);
        var commitments = await _commitmentsRepository.GetByUserAsync(userId);
        var goals = await _goalsRepository.GetByUserAsync(userId);

        var totalBalance = accounts.Sum(x => x.Balance);

        var monthlyCommitments = commitments
            .Where(x => x.IsActive)
            .Sum(x => x.Amount);

        var activeGoals = goals
            .Count(x => !x.IsCompleted);

        var result = new DashboardSummaryResponse
        {
            TotalBalance = totalBalance,
            TotalAccounts = accounts.Count,

            TotalIncome = await _transactionsRepository
                .GetTotalIncomeAsync(userId),

            TotalExpenses = await _transactionsRepository
                .GetTotalExpensesAsync(userId),

            TotalTransactions = await _transactionsRepository
                .GetTotalTransactionsAsync(userId),

            MonthlyCommitments = monthlyCommitments,
            AvailableBalance = totalBalance - monthlyCommitments,
            ActiveGoals = activeGoals
        };

        _logger.LogInformation(
            "Dashboard consultado. UserId: {UserId}, TotalBalance: {TotalBalance}, MonthlyCommitments: {MonthlyCommitments}",
            userId,
            result.TotalBalance,
            result.MonthlyCommitments);

        return result;
    }
}
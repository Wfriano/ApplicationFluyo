using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Dashboard.Dtos;
using FluyoV2.Features.Transactions.Repositories;

namespace FluyoV2.Features.Dashboard.Services;

public class DashboardService
{
    private readonly AccountsRepository _accountsRepository;
    private readonly TransactionsRepository _transactionsRepository;

    public DashboardService(
        AccountsRepository accountsRepository,
        TransactionsRepository transactionsRepository)
    {
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
    }

    public async Task<DashboardSummaryResponse>
        GetSummaryAsync(string userId)
    {
        var accounts =
            await _accountsRepository
                .GetByUserIdAsync(userId);

        return new DashboardSummaryResponse
        {
            TotalBalance =
                accounts.Sum(x => x.Balance),

            TotalAccounts =
                accounts.Count,

            TotalIncome =
                await _transactionsRepository
                    .GetTotalIncomeAsync(userId),

            TotalExpenses =
                await _transactionsRepository
                    .GetTotalExpensesAsync(userId),

            TotalTransactions =
                await _transactionsRepository
                    .GetTotalTransactionsAsync(userId)
        };
    }
}
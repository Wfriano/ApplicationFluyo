using FluyoV2.Features.Transactions.Repositories;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Transactions.Services;
using FluyoV2.Features.Accounts.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace FluyoV2.BackgroundServices;

public class RecurrenceProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecurrenceProcessorService> _logger;

    public RecurrenceProcessorService(IServiceScopeFactory scopeFactory, ILogger<RecurrenceProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecurrenceProcessorService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                using var scope = _scopeFactory.CreateScope();
                var _recurrencesRepository = scope.ServiceProvider.GetRequiredService<RecurrencesRepository>();
                var _transactionsRepository = scope.ServiceProvider.GetRequiredService<TransactionsRepository>();
                var _accountsRepository = scope.ServiceProvider.GetRequiredService<AccountsRepository>();

                var due = await _recurrencesRepository.GetDueRecurrencesAsync(now);

                foreach (var rec in due)
                {
                    // check end date
                    if (rec.EndDate.HasValue && rec.EndDate.Value < now)
                    {
                        // skip or delete
                        continue;
                    }

                    // create transaction
                    var tx = new Transaction
                    {
                        UserId = rec.UserId,
                        AccountId = rec.AccountId,
                        Category = rec.Category,
                        Type = rec.Type,
                        Amount = rec.Amount,
                        Description = rec.Description,
                        TransactionDate = rec.NextDate
                    };

                    await _transactionsRepository.CreateAsync(tx);

                    // update account balance
                    var account = await _accountsRepository.GetByIdAsync(rec.AccountId, rec.UserId);
                    if (account != null)
                    {
                        if (string.Equals(rec.Type, "Income", StringComparison.OrdinalIgnoreCase))
                            account.Balance += rec.Amount;
                        else
                            account.Balance -= rec.Amount;

                        await _accountsRepository.UpdateBalanceAsync(account.Id, account.Balance);
                    }

                    // advance next date according to frequency
                    rec.NextDate = CalculateNext(rec.NextDate, rec.Frequency);

                    await _recurrencesRepository.UpdateAsync(rec);

                    _logger.LogInformation("Processed recurrence {RecId} for user {UserId}", rec.Id, rec.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurrences");
            }

            // Wait one minute before next check (adjust as needed)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private static DateTime CalculateNext(DateTime current, Frequency freq)
    {
        return freq switch
        {
            Frequency.Semanal => current.AddDays(7),
            Frequency.Quincenal => current.AddDays(15),
            Frequency.Mensual => current.AddMonths(1),
            _ => current.AddMonths(1)
        };
    }
}

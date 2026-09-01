using FluyoV2.Constants;
using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Commitments.Models;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Transactions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluyoV2.BackgroundServices;

public class RecurrenceProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecurrenceProcessorService> _logger;

    public RecurrenceProcessorService(
        IServiceScopeFactory scopeFactory,
        ILogger<RecurrenceProcessorService> logger)
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

                if (now.Day == 1)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var recurrencesRepository = scope.ServiceProvider.GetRequiredService<RecurrencesRepository>();
                    var commitmentsRepository = scope.ServiceProvider.GetRequiredService<CommitmentsRepository>();
                    var transactionsRepository = scope.ServiceProvider.GetRequiredService<TransactionsRepository>();
                    var accountsRepository = scope.ServiceProvider.GetRequiredService<AccountsRepository>();

                    var due = await recurrencesRepository.GetDueRecurrencesAsync(now);

                    foreach (var rec in due)
                    {
                        if (rec.EndDate.HasValue && rec.EndDate.Value.Date < now.Date)
                            continue;

                        if (string.Equals(rec.Type, TransactionTypes.Income, StringComparison.OrdinalIgnoreCase))
                        {
                            var transaction = new Transaction
                            {
                                UserId = rec.UserId,
                                AccountId = rec.AccountId,
                                Category = rec.Category,
                                Type = TransactionTypes.Income,
                                Amount = rec.Amount,
                                Description = rec.Description,
                                TransactionDate = now.Date
                            };

                            await transactionsRepository.CreateAsync(transaction);

                            var account = await accountsRepository.GetByIdAsync(rec.AccountId, rec.UserId);
                            if (account is not null)
                            {
                                account.Balance += rec.Amount;
                                await accountsRepository.UpdateBalanceAsync(account.Id, account.Balance);
                            }

                            _logger.LogInformation(
                                "Recurrence converted to income movement. RecurrenceId: {RecurrenceId}, UserId: {UserId}",
                                rec.Id,
                                rec.UserId);
                        }
                        else
                        {
                            var userCommitments = await commitmentsRepository.GetByUserAsync(rec.UserId);

                            var existing = userCommitments
                                .FirstOrDefault(x =>
                                    x.IsActive
                                    && x.PaymentDate.HasValue
                                    && x.PaymentDate.Value.Year == now.Year
                                    && x.PaymentDate.Value.Month == now.Month
                                    && (x.RecurrenceId == rec.Id
                                        || IsLegacyRecurrenceNote(x.Notes, rec.Id)));

                            if (existing is null)
                            {
                                var commitment = new Commitment
                                {
                                    UserId = rec.UserId,
                                    Name = string.IsNullOrWhiteSpace(rec.Description)
                                        ? $"Compromiso recurrente {rec.Category}"
                                        : rec.Description,
                                    Category = string.IsNullOrWhiteSpace(rec.Category)
                                        ? "Compromiso pendiente"
                                        : rec.Category,
                                    Amount = rec.Amount,
                                    PaymentDate = now.Date,
                                    Notes = rec.Note ?? string.Empty,
                                    RecurrenceId = rec.Id
                                };

                                await commitmentsRepository.CreateAsync(commitment);
                            }

                            _logger.LogInformation(
                                "Recurrence converted to pending commitment. RecurrenceId: {RecurrenceId}, UserId: {UserId}",
                                rec.Id,
                                rec.UserId);
                        }

                        rec.NextDate = FirstDayOfNextMonthUtc(now);
                        await recurrencesRepository.UpdateAsync(rec);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurrences");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private static DateTime FirstDayOfNextMonthUtc(DateTime reference)
    {
        return new DateTime(
            reference.Year,
            reference.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc).AddMonths(1);
    }

    private static bool IsLegacyRecurrenceNote(string? notes, string recurrenceId)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return false;

        return notes.Contains($"Recurrence:{recurrenceId}:", StringComparison.OrdinalIgnoreCase);
    }
}

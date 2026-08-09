using FluyoV2.Features.Transactions.Dtos;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Transactions.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Transactions.Services;

public class RecurrencesService
{
    private readonly RecurrencesRepository _repository;
    private readonly ILogger<RecurrencesService> _logger;

    public RecurrencesService(
        RecurrencesRepository repository,
        ILogger<RecurrencesService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<RecurrenceResponse> CreateAsync(
        string userId,
        CreateRecurrenceRequest request)
    {
        if (!Enum.TryParse<Frequency>(request.Frequency, ignoreCase: true, out var frequency))
            throw new ArgumentException("Frequency is invalid");

        var recurrence = new Recurrence
        {
            TransactionId = request.TransactionId,
            UserId = userId,
            Frequency = frequency,
            NextDate = FirstDayOfSelectedMonthUtc(request.NextDate),
            EndDate = request.EndDate,
            Amount = request.Amount,
            Type = request.Type,
            Category = request.Category,
            Description = request.Description,
            AccountId = request.AccountId,
            IsPaid = request.IsPaid,
            Note = request.Note ?? string.Empty
        };

        await _repository.CreateAsync(recurrence);

        _logger.LogInformation(
            "Recurrence created. UserId: {UserId}, TransactionId: {TransactionId}, NextDate: {NextDate}",
            userId,
            request.TransactionId,
            recurrence.NextDate);

        return Map(recurrence);
    }

    public async Task<RecurrenceResponse?> GetByTransactionIdAsync(string userId, string transactionId)
    {
        var recurrence = await _repository.GetByTransactionIdAsync(transactionId);

        if (recurrence is null || recurrence.UserId != userId)
            return null;

        return Map(recurrence);
    }

    public async Task<List<RecurrenceResponse>> GetAllByUserAsync(string userId)
    {
        var items = await _repository.GetByUserAsync(userId);
        return items.Select(Map).ToList();
    }

    public async Task UpdateAsync(string userId, RecurrenceResponse request)
    {
        var recurrence = await _repository.GetByTransactionIdAsync(request.TransactionId);

        if (recurrence is null || recurrence.UserId != userId)
            throw new ArgumentException("Recurrence not found or unauthorized");

        if (!Enum.TryParse<Frequency>(request.Frequency, true, out var frequency))
            throw new ArgumentException("Frequency is invalid");

        recurrence.Frequency = frequency;
        recurrence.NextDate = FirstDayOfSelectedMonthUtc(request.NextDate);
        recurrence.EndDate = request.EndDate;
        recurrence.Amount = request.Amount;
        recurrence.Type = request.Type;
        recurrence.Category = request.Category;
        recurrence.Description = request.Description;
        recurrence.AccountId = request.AccountId;
        recurrence.IsPaid = request.IsPaid;
        recurrence.Note = request.Note ?? string.Empty;

        await _repository.UpdateAsync(recurrence);
    }

    public async Task DeleteAsync(string userId, string id)
    {
        var items = await _repository.GetByUserAsync(userId);
        var rec = items.FirstOrDefault(x => x.Id == id);

        if (rec is null)
            throw new ArgumentException("Recurrence not found or unauthorized");

        await _repository.DeleteAsync(id);
    }

    private static DateTime FirstDayOfSelectedMonthUtc(DateTime selected)
    {
        var source = selected == default
            ? DateTime.UtcNow.AddMonths(1)
            : selected;

        return new DateTime(
            source.Year,
            source.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);
    }

    private static string? NormalizeOptionalId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value;
    }

    private static RecurrenceResponse Map(Recurrence r)
    {
        return new RecurrenceResponse
        {
            Id = r.Id,
            TransactionId = r.TransactionId,
            Frequency = r.Frequency.ToString(),
            NextDate = r.NextDate,
            EndDate = r.EndDate,
            CreatedAt = r.CreatedAt,
            Amount = r.Amount,
            Type = r.Type,
            Category = r.Category,
            Description = r.Description,
            AccountId = r.AccountId,
            IsPaid = r.IsPaid,
            Note = r.Note
        };
    }
}

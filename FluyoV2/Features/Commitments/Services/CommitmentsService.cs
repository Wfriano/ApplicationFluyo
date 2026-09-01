using FluyoV2.Constants;
using FluyoV2.Exceptions;
using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Commitments.Dtos;
using FluyoV2.Features.Commitments.Models;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Transactions.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Commitments.Services;

public class CommitmentsService
{
    private readonly CommitmentsRepository _repository;
    private readonly AccountsRepository _accountsRepository;
    private readonly TransactionsRepository _transactionsRepository;
    private readonly RecurrencesRepository _recurrencesRepository;
    private readonly ILogger<CommitmentsService> _logger;

    public CommitmentsService(
        CommitmentsRepository repository,
        AccountsRepository accountsRepository,
        TransactionsRepository transactionsRepository,
        RecurrencesRepository recurrencesRepository,
        ILogger<CommitmentsService> logger)
    {
        _repository = repository;
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
        _recurrencesRepository = recurrencesRepository;
        _logger = logger;
    }

    public async Task<CommitmentResponse> CreateAsync(
        string userId,
        CreateCommitmentRequest request)
    {
        // avoid duplicate commitments with same name, payment date and amount
        var existing = (await _repository.GetByUserAsync(userId))
            .FirstOrDefault(x => x.Name == request.Name
                && x.Amount == request.Amount
                && x.Category == request.Category
                && ((x.PaymentDate.HasValue && request.PaymentDate.HasValue && x.PaymentDate.Value.Date == request.PaymentDate.Value.Date)
                    || (!x.PaymentDate.HasValue && !request.PaymentDate.HasValue))
                && (string.IsNullOrEmpty(x.Notes) && string.IsNullOrEmpty(request.Notes) || x.Notes == request.Notes));

        if (existing is not null)
        {
            _logger.LogInformation(
                "Compromiso ya existe. UserId: {UserId}, CommitmentId: {CommitmentId}, Name: {Name}",
                userId,
                existing.Id,
                existing.Name);

            return Map(existing);
        }

        var commitment = new Commitment
        {
            UserId = userId,
            Name = request.Name,
            Category = request.Category,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            Notes = request.Notes ?? string.Empty
        };

        await _repository.CreateAsync(commitment);

        _logger.LogInformation(
            "Compromiso creado. UserId: {UserId}, CommitmentId: {CommitmentId}, Name: {Name}",
            userId,
            commitment.Id,
            commitment.Name);

        // If client provided recurrence settings, create a recurrence so the background processor will generate pending commitments
        if (request.Recurrence != null && (
            !string.IsNullOrWhiteSpace(request.Recurrence.Frequency) ||
            request.Recurrence.NextDate > DateTime.MinValue ||
            request.Recurrence.Amount > 0 ||
            !string.IsNullOrWhiteSpace(request.Recurrence.Type) ||
            !string.IsNullOrWhiteSpace(request.Recurrence.AccountId)
        ))
        {
            try
            {
                // Validate frequency
                if (!Enum.TryParse<FluyoV2.Features.Transactions.Models.Frequency>(request.Recurrence.Frequency, true, out var frequency))
                {
                    throw new ArgumentException("Frequency is invalid");
                }

                var recurrence = new FluyoV2.Features.Transactions.Models.Recurrence
                {
                    TransactionId = null,
                    UserId = userId,
                    Frequency = frequency,
                    NextDate = FirstDayOfSelectedMonthUtc(request.Recurrence.NextDate),
                    EndDate = request.Recurrence.EndDate,
                    Amount = request.Recurrence.Amount > 0 ? request.Recurrence.Amount : request.Amount,
                    Type = "Expense",
                    Category = request.Category,
                    Description = string.IsNullOrWhiteSpace(request.Recurrence.Description) ? request.Name : request.Recurrence.Description,
                    AccountId = request.Recurrence.AccountId ?? string.Empty,
                    IsPaid = request.Recurrence.IsPaid,
                    Note = request.Notes ?? string.Empty
                };

                await _recurrencesRepository.CreateAsync(recurrence);

                // attach recurrence marker to the commitment notes so we can find the recurrence later
                var marker = $"Recurrence:{recurrence.Id}:orig";
                commitment.Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? marker
                    : (request.Notes + " " + marker).Trim();

                await _repository.UpdateAsync(commitment);

                _logger.LogInformation(
                    "Recurrence created for commitment. UserId: {UserId}, CommitmentId: {CommitmentId}, NextDate: {NextDate}",
                    userId,
                    commitment.Id,
                    recurrence.NextDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create recurrence for commitment. UserId: {UserId}, CommitmentId: {CommitmentId}", userId, commitment.Id);
                // swallow exception so commitment creation still succeeds; client can retry recurrence creation separately
            }
        }

        return Map(commitment, null);
    }

    public async Task<List<CommitmentResponse>> GetAllAsync(
        string userId)
    {
        var commitments = await _repository.GetByUserAsync(userId);
        var recurrences = await _recurrencesRepository.GetByUserAsync(userId);

        _logger.LogInformation(
            "Compromisos consultados. UserId: {UserId}, Total: {Total}",
            userId,
            commitments.Count);

        var recById = recurrences.ToDictionary(r => r.Id, r => new FluyoV2.Features.Transactions.Dtos.RecurrenceResponse
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
            OtherAccountId = null,
            IsPaid = r.IsPaid,
            Note = r.Note
        });

        // return only active commitments, include recurrence info when available
        return commitments
            .Where(c => c.IsActive)
            .Select(c =>
            {
                var recurrenceId = ExtractRecurrenceId(c.Notes);
                recById.TryGetValue(recurrenceId ?? string.Empty, out var rec);
                return Map(c, rec);
            })
            .ToList();
    }

    public async Task<List<UpcomingCommitmentResponse>> GetUpcomingAsync(
        string userId,
        int? month = null,
        int? year = null)
    {
        var commitments = await _repository.GetByUserAsync(userId);

        var now = DateTime.UtcNow;
        var targetMonth = month ?? now.Month;
        var targetYear = year ?? now.Year;

        var upcoming = commitments
            .Where(c => c.IsActive && c.PaymentDate.HasValue)
            .Select(c =>
            {
                // compute next due date for the requested month using original payment day
                var daysInMonth = DateTime.DaysInMonth(targetYear, targetMonth);
                var day = Math.Min(c.PaymentDate.Value.Day, daysInMonth);
                var dueDate = new DateTime(targetYear, targetMonth, day);

                var paidThisMonth = c.LastPaymentDate.HasValue
                    && c.LastPaymentDate.Value.Month == targetMonth
                    && c.LastPaymentDate.Value.Year == targetYear;

                return new UpcomingCommitmentResponse
                {
                    Id = c.Id,
                    AccountId = c.AccountId,
                    Name = c.Name,
                    Category = c.Category,
                    Amount = c.Amount,
                    DueDate = dueDate,
                    IsPaid = paidThisMonth
                    ,
                    Notes = c.Notes ?? string.Empty
                };
            })
            // only those not paid this month
            .Where(x => !x.IsPaid)
            .OrderBy(x => x.DueDate)
            .ToList();

        _logger.LogInformation(
            "Próximos compromisos consultados. UserId: {UserId}, Count: {Count}",
            userId,
            upcoming.Count);

        return upcoming;
    }

    public async Task<CommitmentResponse?> GetByIdAsync(
        string id,
        string userId)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
            return null;

        var recurrences = await _recurrencesRepository.GetByUserAsync(userId);
        var recurrenceId = ExtractRecurrenceId(commitment.Notes);
        var rec = recurrences.FirstOrDefault(r => r.Id == recurrenceId);

        FluyoV2.Features.Transactions.Dtos.RecurrenceResponse? recResp = null;
        if (rec is not null)
        {
            recResp = new FluyoV2.Features.Transactions.Dtos.RecurrenceResponse
            {
                Id = rec.Id,
                TransactionId = rec.TransactionId,
                Frequency = rec.Frequency.ToString(),
                NextDate = rec.NextDate,
                EndDate = rec.EndDate,
                CreatedAt = rec.CreatedAt,
                Amount = rec.Amount,
                Type = rec.Type,
                Category = rec.Category,
                Description = rec.Description,
                AccountId = rec.AccountId,
                OtherAccountId = null,
                IsPaid = rec.IsPaid,
                Note = rec.Note
            };
        }

        return Map(commitment, recResp);
    }

    public async Task<decimal> GetPendingTotalAsync(string userId)
    {
        var commitments = await _repository.GetByUserAsync(userId);

        var total = commitments
            .Where(c => c.IsActive)
            .Sum(c => c.Amount);

        _logger.LogInformation(
            "Balance pendiente calculado. UserId: {UserId}, Total: {Total}",
            userId,
            total);

        return total;
    }

    public async Task<CommitmentResponse?> UpdateAsync(
        string id,
        string userId,
        UpdateCommitmentRequest request)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
            return null;

        // keep existing notes to possibly restore/modify after recurrence handling
        var originalNotes = commitment.Notes ?? string.Empty;

        commitment.Name = request.Name;
        commitment.Category = request.Category;
        commitment.Amount = request.Amount;
        commitment.PaymentDate = request.PaymentDate;
        commitment.IsActive = request.IsActive;

        await _repository.UpdateAsync(commitment);

        _logger.LogInformation(
            "Compromiso actualizado. UserId: {UserId}, CommitmentId: {CommitmentId}",
            userId,
            commitment.Id);

        // handle recurrence update/create/delete
        var existingRecurrenceId = ExtractRecurrenceId(originalNotes);

        var hasRecurrenceData = request.Recurrence != null && (
            !string.IsNullOrWhiteSpace(request.Recurrence.Frequency) ||
            request.Recurrence.NextDate > DateTime.MinValue ||
            request.Recurrence.Amount > 0 ||
            !string.IsNullOrWhiteSpace(request.Recurrence.Type) ||
            !string.IsNullOrWhiteSpace(request.Recurrence.AccountId)
        );

        if (hasRecurrenceData)
        {
            // create or update recurrence
            if (!Enum.TryParse<FluyoV2.Features.Transactions.Models.Frequency>(request.Recurrence.Frequency, true, out var frequency))
                throw new ArgumentException("Frequency is invalid");

            if (string.IsNullOrWhiteSpace(existingRecurrenceId))
            {
                var recurrence = new FluyoV2.Features.Transactions.Models.Recurrence
                {
                    TransactionId = null,
                    UserId = userId,
                    Frequency = frequency,
                    NextDate = FirstDayOfSelectedMonthUtc(request.Recurrence.NextDate),
                    EndDate = request.Recurrence.EndDate,
                    Amount = request.Recurrence.Amount > 0 ? request.Recurrence.Amount : request.Amount,
                    Type = "Expense",
                    Category = request.Category,
                    Description = string.IsNullOrWhiteSpace(request.Recurrence.Description) ? request.Name : request.Recurrence.Description,
                    AccountId = request.Recurrence.AccountId ?? string.Empty,
                    IsPaid = request.Recurrence.IsPaid,
                    Note = request.Notes ?? string.Empty
                };

                await _recurrencesRepository.CreateAsync(recurrence);

                var marker = $"Recurrence:{recurrence.Id}:orig";
                commitment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? marker : (request.Notes + " " + marker).Trim();
                await _repository.UpdateAsync(commitment);
            }
            else
            {
                // update existing recurrence if found
                var recList = await _recurrencesRepository.GetByUserAsync(userId);
                var existingRec = recList.FirstOrDefault(r => r.Id == existingRecurrenceId);

                if (existingRec is not null)
                {
                    existingRec.Frequency = frequency;
                    existingRec.NextDate = FirstDayOfSelectedMonthUtc(request.Recurrence.NextDate);
                    existingRec.EndDate = request.Recurrence.EndDate;
                    existingRec.Amount = request.Recurrence.Amount > 0 ? request.Recurrence.Amount : request.Amount;
                    existingRec.Type = "Expense";
                    existingRec.Category = request.Category;
                    existingRec.Description = string.IsNullOrWhiteSpace(request.Recurrence.Description) ? request.Name : request.Recurrence.Description;
                    existingRec.AccountId = request.Recurrence.AccountId ?? string.Empty;
                    existingRec.IsPaid = request.Recurrence.IsPaid;
                    existingRec.Note = request.Notes ?? string.Empty;

                    await _recurrencesRepository.UpdateAsync(existingRec);
                }
                else
                {
                    // fallback: create new recurrence if the id wasn't found
                    var recurrence = new FluyoV2.Features.Transactions.Models.Recurrence
                    {
                        TransactionId = null,
                        UserId = userId,
                        Frequency = frequency,
                        NextDate = FirstDayOfSelectedMonthUtc(request.Recurrence.NextDate),
                        EndDate = request.Recurrence.EndDate,
                        Amount = request.Recurrence.Amount > 0 ? request.Recurrence.Amount : request.Amount,
                        Type = "Expense",
                        Category = request.Category,
                        Description = string.IsNullOrWhiteSpace(request.Recurrence.Description) ? request.Name : request.Recurrence.Description,
                        AccountId = request.Recurrence.AccountId ?? string.Empty,
                        IsPaid = request.Recurrence.IsPaid,
                        Note = request.Notes ?? string.Empty
                    };

                    await _recurrencesRepository.CreateAsync(recurrence);

                    var marker = $"Recurrence:{recurrence.Id}:orig";
                    commitment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? marker : (request.Notes + " " + marker).Trim();
                    await _repository.UpdateAsync(commitment);
                }
            }
        }
        else
        {
            // client removed recurrence data: if there was an existing recurrence, delete it and remove marker from notes
            if (!string.IsNullOrWhiteSpace(existingRecurrenceId))
            {
                await _recurrencesRepository.DeleteAsync(existingRecurrenceId);
                // remove any Recurrence:<id>:... fragment from notes
                commitment.Notes = RemoveRecurrenceMarker(commitment.Notes ?? string.Empty, existingRecurrenceId);
                await _repository.UpdateAsync(commitment);
            }
        }

        // include recurrence (if any) in the response
        var recId = ExtractRecurrenceId(commitment.Notes);
        FluyoV2.Features.Transactions.Dtos.RecurrenceResponse? recResp = null;
        if (!string.IsNullOrWhiteSpace(recId))
        {
            var rec = (await _recurrencesRepository.GetByUserAsync(commitment.UserId)).FirstOrDefault(r => r.Id == recId);
            if (rec is not null)
            {
                recResp = new FluyoV2.Features.Transactions.Dtos.RecurrenceResponse
                {
                    Id = rec.Id,
                    TransactionId = rec.TransactionId,
                    Frequency = rec.Frequency.ToString(),
                    NextDate = rec.NextDate,
                    EndDate = rec.EndDate,
                    CreatedAt = rec.CreatedAt,
                    Amount = rec.Amount,
                    Type = rec.Type,
                    Category = rec.Category,
                    Description = rec.Description,
                    AccountId = rec.AccountId,
                    OtherAccountId = null,
                    IsPaid = rec.IsPaid,
                    Note = rec.Note
                };
            }
        }

        return Map(commitment, recResp);
    }

    public async Task<bool> DeleteAsync(
        string id,
        string userId)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
            return false;

        await _repository.DeleteAsync(id);

        _logger.LogInformation(
            "Compromiso eliminado. UserId: {UserId}, CommitmentId: {CommitmentId}",
            userId,
            id);

        return true;
    }

    public async Task<bool> DeleteRecurringSeriesAsync(
        string id,
        string userId)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
            return false;

        var recurrenceId = ExtractRecurrenceId(commitment.Notes);

        // Elimina el compromiso actual
        await _repository.DeleteAsync(id);

        if (!string.IsNullOrWhiteSpace(recurrenceId))
        {
            // Elimina otros compromisos generados por la misma recurrencia
            var userCommitments = await _repository.GetByUserAsync(userId);

            var relatedCommitments = userCommitments
                .Where(x => x.Id != id
                    && x.IsActive
                    && IsFromRecurrence(x.Notes, recurrenceId))
                .ToList();

            foreach (var item in relatedCommitments)
            {
                await _repository.DeleteAsync(item.Id);
            }

            // Elimina la configuración de recurrencia para que no vuelva a generarse a futuro
            await _recurrencesRepository.DeleteAsync(recurrenceId);
        }

        _logger.LogInformation(
            "Serie recurrente eliminada. UserId: {UserId}, CommitmentId: {CommitmentId}, RecurrenceId: {RecurrenceId}",
            userId,
            id,
            recurrenceId ?? string.Empty);

        return true;
    }

    public async Task<CommitmentResponse?> PayCommitmentAsync(
        string id,
        string userId,
        FluyoV2.Features.Commitments.Dtos.PayCommitmentRequest? request)
    {
        var commitment = await _repository.GetByIdAsync(id);

        if (commitment is null || commitment.UserId != userId)
        {
            _logger.LogWarning(
                "Compromiso no encontrado. UserId: {UserId}, CommitmentId: {CommitmentId}",
                userId,
                id);

            return null;
        }

        var accountIdToUse = request?.AccountId;
        if (string.IsNullOrEmpty(accountIdToUse))
            accountIdToUse = commitment.AccountId;

        if (string.IsNullOrEmpty(accountIdToUse))
        {
            _logger.LogWarning(
                "No se especificó cuenta para pagar el compromiso. UserId: {UserId}, CommitmentId: {CommitmentId}",
                userId,
                id);

            throw new BusinessException(
                "Debe seleccionar una cuenta para pagar el compromiso");
        }

        var account = await _accountsRepository.GetByIdAsync(
            accountIdToUse,
            userId);

        if (account is null)
        {
            _logger.LogWarning(
                "Cuenta no encontrada para compromiso. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                commitment.AccountId);

            return null;
        }

        // determine payment date (use provided one or now)
        var paymentDate = request?.PaymentDate ?? DateTime.UtcNow;

        if (commitment.LastPaymentDate.HasValue)
        {
            var lastPayment = commitment.LastPaymentDate.Value;

            if (lastPayment.Month == paymentDate.Month &&
                lastPayment.Year == paymentDate.Year)
            {
                _logger.LogWarning(
                    "Compromiso ya pagado este mes. UserId: {UserId}, CommitmentId: {CommitmentId}",
                    userId,
                    commitment.Id);

                throw new BusinessException(
                    "Este compromiso ya fue pagado este mes");
            }
        }

        if (account.Balance < commitment.Amount)
        {
            _logger.LogWarning(
                "Saldo insuficiente para pagar compromiso. UserId: {UserId}, AccountId: {AccountId}, Balance: {Balance}, Amount: {Amount}",
                userId,
                account.Id,
                account.Balance,
                commitment.Amount);

            throw new BusinessException(
                "Saldo insuficiente para pagar el compromiso");
        }

        account.Balance -= commitment.Amount;

        await _accountsRepository.UpdateBalanceAsync(
            account.Id,
            account.Balance);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = account.Id,
            Category = commitment.Category,
            Type = TransactionTypes.Expense,
            Amount = commitment.Amount,
            Description = $"Pago automático: {commitment.Name}",
            TransactionDate = paymentDate
        };

        await _transactionsRepository.CreateAsync(transaction);

        commitment.LastPaymentDate = paymentDate;

        // after creating the transaction, remove the commitment from pending list
        await _repository.DeleteAsync(commitment.Id);

        _logger.LogInformation(
            "Compromiso pagado y eliminado. UserId: {UserId}, CommitmentId: {CommitmentId}, Amount: {Amount}",
            userId,
            commitment.Id,
            commitment.Amount);

        // return the commitment info that was paid
        return Map(commitment, null);
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

    private static bool IsFromRecurrence(string? notes, string recurrenceId)
    {
        return ExtractRecurrenceId(notes) == recurrenceId;
    }

    private static string? ExtractRecurrenceId(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;

        // Expected format: Recurrence:{recurrenceId}:{yyyy-MM}
        var parts = notes.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            return null;

        if (!parts[0].Equals("Recurrence", StringComparison.OrdinalIgnoreCase))
            return null;

        return parts[1];
    }

    private static CommitmentResponse Map(
        Commitment commitment,
        FluyoV2.Features.Transactions.Dtos.RecurrenceResponse? recurrence = null)
    {
        return new CommitmentResponse
        {
            Id = commitment.Id,
            AccountId = commitment.AccountId,
            Name = commitment.Name,
            Category = commitment.Category,
            Amount = commitment.Amount,
            PaymentDate = commitment.PaymentDate,
            IsActive = commitment.IsActive,
            Notes = commitment.Notes ?? string.Empty,
            LastPaymentDate = commitment.LastPaymentDate,
            CreatedAt = commitment.CreatedAt,
            Recurrence = recurrence
        };
    }

    private static string RemoveRecurrenceMarker(string notes, string recurrenceId)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return notes;

        var markerPrefix = $"Recurrence:{recurrenceId}:";
        var idx = notes.IndexOf(markerPrefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return notes.Trim();

        // remove marker and following token until whitespace or end
        var endIdx = notes.IndexOf(' ', idx);
        if (endIdx < 0)
            endIdx = notes.Length;

        var newNotes = notes.Remove(idx, endIdx - idx).Trim();

        return newNotes;
    }
}


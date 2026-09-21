using FluyoV2.Features.Notifications.Dtos;
using FluyoV2.Features.Notifications.Models;
using FluyoV2.Features.Notifications.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Notifications.Services;

public class NotificationsService
{
    private readonly NotificationsRepository _repository;
    private readonly NotificationDevicesRepository _devicesRepository;
    private readonly ExpoPushService _expoPushService;
    private readonly EmotionalCalendarService _calendarService;
    private readonly ILogger<NotificationsService> _logger;

    public NotificationsService(
        NotificationsRepository repository,
        NotificationDevicesRepository devicesRepository,
        ExpoPushService expoPushService,
        EmotionalCalendarService calendarService,
        ILogger<NotificationsService> logger)
    {
        _repository = repository;
        _devicesRepository = devicesRepository;
        _expoPushService = expoPushService;
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<List<NotificationResponse>> GetAllAsync(string userId)
    {
        var items = await _repository.GetByUserAsync(userId);
        return items.Select(Map).ToList();
    }

    public async Task<bool> MarkAsReadAsync(string userId, string id)
    {
        var item = await _repository.GetByIdAsync(id);

        if (item is null || item.UserId != userId || item.IsDeleted)
            return false;

        item.IsRead = true;
        item.ReadAt = DateTime.UtcNow;

        await _repository.UpdateAsync(item);
        return true;
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _repository.MarkAllAsReadAsync(userId);
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var item = await _repository.GetByIdAsync(id);

        if (item is null || item.UserId != userId || item.IsDeleted)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task DeleteAllAsync(string userId)
    {
        await _repository.DeleteAllAsync(userId);
    }

    public async Task RegisterDeviceAsync(string userId, RegisterNotificationDeviceRequest request)
    {
        var device = new NotificationDevice
        {
            UserId = userId,
            InstallationId = request.InstallationId,
            ExpoPushToken = request.ExpoPushToken,
            Platform = request.Platform,
            TimeZone = string.IsNullOrWhiteSpace(request.TimeZone) ? "UTC" : request.TimeZone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UnregisteredAt = null
        };

        await _devicesRepository.UpsertAsync(device);
    }

    public async Task UnregisterDeviceAsync(string userId, string installationId)
    {
        await _devicesRepository.DeactivateAsync(installationId, userId);
    }

    public async Task CreatePaymentNotificationIfNotExistsAsync(
        string userId,
        string title,
        string message,
        string sourceType,
        string sourceId,
        DateTime paymentDate,
        string dedupKey)
    {
        var existing = await _repository.GetByDedupKeyAsync(dedupKey);
        if (existing is not null)
            return;

        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            SourceType = sourceType,
            SourceId = sourceId,
            PaymentDate = paymentDate,
            DedupKey = dedupKey,
            ScheduledAt = paymentDate,
            Status = "pending"
        };

        await _repository.CreateAsync(notification);

        _logger.LogInformation(
            "Notificación de pago creada. UserId: {UserId}, SourceType: {SourceType}, SourceId: {SourceId}",
            userId,
            sourceType,
            sourceId);
    }

    public async Task EnsureDailyEmotionalCalendarNotificationAsync(string userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var template = _calendarService.GetMessageForDate(date);
        var dedupKey = $"emotional-calendar:{userId}:{date:yyyyMMdd}";
        var existing = await _repository.GetByDedupKeyAsync(dedupKey);

        if (existing is not null)
            return;

        var notification = new Notification
        {
            UserId = userId,
            Title = template.Title,
            Message = template.Message,
            SourceType = "EmotionalCalendar",
            SourceId = template.Day.ToString(),
            PaymentDate = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            DedupKey = dedupKey,
            ScheduledAt = await ResolveScheduledAtUtc(userId, date),
            Status = "pending"
        };

        await _repository.CreateAsync(notification);
    }

    public async Task DispatchPendingAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var pending = await _repository.GetPendingDueAsync(utcNow);

        foreach (var item in pending)
        {
            var devices = await _devicesRepository.GetActiveByUserAsync(item.UserId);

            if (devices.Count == 0)
                continue;

            var sent = false;

            foreach (var device in devices)
            {
                var result = await _expoPushService.SendAsync(
                    device.ExpoPushToken,
                    item.Title,
                    item.Message,
                    new
                    {
                        notificationId = item.Id,
                        sourceType = item.SourceType,
                        sourceId = item.SourceId
                    },
                    cancellationToken);

                sent = sent || result;
            }

            if (sent)
            {
                await _repository.MarkAsSentAsync(item.Id, utcNow);
            }
            else
            {
                await _repository.MarkAsFailedAsync(item.Id, "Expo push no respondió correctamente");
            }
        }
    }

    private async Task<DateTime> ResolveScheduledAtUtc(string userId, DateOnly date)
    {
        var device = (await _devicesRepository.GetActiveByUserAsync(userId)).FirstOrDefault();
        var timeZone = ResolveTimeZone(device?.TimeZone);
        var localScheduled = date.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localScheduled, timeZone);
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            if (string.Equals(timeZoneId, "America/Bogota", StringComparison.OrdinalIgnoreCase))
            {
                return TimeZoneInfo.CreateCustomTimeZone(
                    "America/Bogota",
                    TimeSpan.FromHours(-5),
                    "Bogota",
                    "Bogota");
            }

            return TimeZoneInfo.Utc;
        }
    }

    private static NotificationResponse Map(Notification item)
    {
        return new NotificationResponse
        {
            Id = item.Id,
            Title = item.Title,
            Message = item.Message,
            SourceType = item.SourceType,
            SourceId = item.SourceId,
            PaymentDate = item.PaymentDate,
            IsRead = item.IsRead,
            CreatedAt = item.CreatedAt,
            ReadAt = item.ReadAt
        };
    }
}

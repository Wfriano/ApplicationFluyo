using FluyoV2.Features.Notifications.Dtos;
using FluyoV2.Features.Notifications.Models;
using FluyoV2.Features.Notifications.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Notifications.Services;

public class NotificationsService
{
    private readonly NotificationsRepository _repository;
    private readonly ILogger<NotificationsService> _logger;

    public NotificationsService(
        NotificationsRepository repository,
        ILogger<NotificationsService> logger)
    {
        _repository = repository;
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
            DedupKey = dedupKey
        };

        await _repository.CreateAsync(notification);

        _logger.LogInformation(
            "Notificación de pago creada. UserId: {UserId}, SourceType: {SourceType}, SourceId: {SourceId}",
            userId,
            sourceType,
            sourceId);
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

using FluyoV2.Features.Notifications.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Notifications.Repositories;

public class NotificationsRepository
{
    private readonly MongoDbContext _context;

    public NotificationsRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Notification notification)
    {
        await _context.Notifications.InsertOneAsync(notification);
    }

    public async Task<List<Notification>> GetByUserAsync(string userId)
    {
        return await _context.Notifications
            .Find(x => x.UserId == userId && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetPendingDueAsync(DateTime utcNow)
    {
        return await _context.Notifications
            .Find(x => !x.IsDeleted && x.Status == "pending" && x.ScheduledAt.HasValue && x.ScheduledAt.Value <= utcNow)
            .SortBy(x => x.ScheduledAt)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(string id)
    {
        return await _context.Notifications
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Notification?> GetByDedupKeyAsync(string dedupKey)
    {
        return await _context.Notifications
            .Find(x => x.DedupKey == dedupKey && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Notification notification)
    {
        await _context.Notifications.ReplaceOneAsync(
            x => x.Id == notification.Id,
            notification);
    }

    public async Task MarkAsSentAsync(string id, DateTime sentAt)
    {
        var update = Builders<Notification>.Update
            .Set(x => x.Status, "sent")
            .Set(x => x.SentAt, sentAt)
            .Set(x => x.ErrorMessage, null);

        await _context.Notifications.UpdateOneAsync(x => x.Id == id, update);
    }

    public async Task MarkAsFailedAsync(string id, string errorMessage)
    {
        var update = Builders<Notification>.Update
            .Set(x => x.Status, "failed")
            .Set(x => x.ErrorMessage, errorMessage);

        await _context.Notifications.UpdateOneAsync(x => x.Id == id, update);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Find(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
            .ToListAsync();

        foreach (var item in notifications)
        {
            item.IsRead = true;
            item.ReadAt = DateTime.UtcNow;
            await UpdateAsync(item);
        }
    }

    public async Task DeleteAsync(string id)
    {
        var notification = await GetByIdAsync(id);
        if (notification is null)
            return;

        notification.IsDeleted = true;
        await UpdateAsync(notification);
    }

    public async Task DeleteAllAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Find(x => x.UserId == userId && !x.IsDeleted)
            .ToListAsync();

        foreach (var item in notifications)
        {
            item.IsDeleted = true;
            await UpdateAsync(item);
        }
    }
}

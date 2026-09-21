using FluyoV2.Features.Notifications.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Notifications.Repositories;

public class NotificationDevicesRepository
{
    private readonly MongoDbContext _context;

    public NotificationDevicesRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationDevice?> GetByInstallationIdAsync(string installationId)
    {
        return await _context.NotificationDevices
            .Find(x => x.InstallationId == installationId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<NotificationDevice>> GetActiveByUserAsync(string userId)
    {
        return await _context.NotificationDevices
            .Find(x => x.UserId == userId && x.IsActive)
            .ToListAsync();
    }

    public async Task<List<NotificationDevice>> GetAllActiveAsync()
    {
        return await _context.NotificationDevices
            .Find(x => x.IsActive)
            .ToListAsync();
    }

    public async Task UpsertAsync(NotificationDevice device)
    {
        device.UpdatedAt = DateTime.UtcNow;

        await _context.NotificationDevices.ReplaceOneAsync(
            x => x.InstallationId == device.InstallationId,
            device,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task DeactivateAsync(string installationId, string userId)
    {
        var device = await _context.NotificationDevices
            .Find(x => x.InstallationId == installationId && x.UserId == userId)
            .FirstOrDefaultAsync();

        if (device is null)
            return;

        device.IsActive = false;
        device.UnregisteredAt = DateTime.UtcNow;
        device.UpdatedAt = DateTime.UtcNow;

        await UpsertAsync(device);
    }
}

using FluyoV2.Features.Assets.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Assets.Repositories;

public class AssetsRepository
{
    private readonly MongoDbContext _context;

    public AssetsRepository(
        MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        Asset asset)
    {
        await _context.Assets
            .InsertOneAsync(asset);
    }

    public async Task<List<Asset>>
        GetByUserAsync(string userId)
    {
        return await _context.Assets
            .Find(x => x.UserId == userId && x.IsActive)
            .ToListAsync();
    }

    public async Task<Asset?>
        GetByIdAsync(string id)
    {
        return await _context.Assets
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(
        Asset asset)
    {
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.Assets
            .ReplaceOneAsync(
                x => x.Id == asset.Id,
                asset);
    }

    public async Task DeleteAsync(string id)
    {
        var asset = await GetByIdAsync(id);
        if (asset != null)
        {
            asset.IsActive = false;
            asset.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(asset);
        }
    }
}

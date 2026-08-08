using FluyoV2.Features.Liabilities.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Liabilities.Repositories;

public class LiabilitiesRepository
{
    private readonly MongoDbContext _context;

    public LiabilitiesRepository(
        MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        Liability liability)
    {
        await _context.Liabilities
            .InsertOneAsync(liability);
    }

    public async Task<List<Liability>>
        GetByUserAsync(string userId)
    {
        return await _context.Liabilities
            .Find(x => x.UserId == userId && x.IsActive)
            .ToListAsync();
    }

    public async Task<Liability?>
        GetByIdAsync(string id)
    {
        return await _context.Liabilities
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(
        Liability liability)
    {
        liability.UpdatedAt = DateTime.UtcNow;
        await _context.Liabilities
            .ReplaceOneAsync(
                x => x.Id == liability.Id,
                liability);
    }

    public async Task DeleteAsync(string id)
    {
        var liability = await GetByIdAsync(id);
        if (liability != null)
        {
            liability.IsActive = false;
            liability.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(liability);
        }
    }
}

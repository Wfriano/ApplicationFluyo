using FluyoV2.Features.Goals.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Goals.Repositories;

public class GoalsRepository
{
    private readonly MongoDbContext _context;

    public GoalsRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Goal goal)
    {
        await _context.Goals.InsertOneAsync(goal);
    }

    public async Task<List<Goal>> GetByUserAsync(
        string userId)
    {
        return await _context.Goals
            .Find(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<Goal?> GetByIdAsync(
        string id)
    {
        return await _context.Goals
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Goal goal)
    {
        await _context.Goals.ReplaceOneAsync(
            x => x.Id == goal.Id,
            goal);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Goals.DeleteOneAsync(
            x => x.Id == id);
    }
}
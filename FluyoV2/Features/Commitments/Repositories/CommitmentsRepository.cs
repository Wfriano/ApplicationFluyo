using FluyoV2.Features.Commitments.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Commitments.Repositories;

public class CommitmentsRepository
{
    private readonly MongoDbContext _context;

    public CommitmentsRepository(
        MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        Commitment commitment)
    {
        await _context.Commitments
            .InsertOneAsync(commitment);
    }

    public async Task<List<Commitment>>
        GetByUserAsync(string userId)
    {
        return await _context.Commitments
            .Find(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<Commitment?>
        GetByIdAsync(string id)
    {
        return await _context.Commitments
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(
        Commitment commitment)
    {
        await _context.Commitments
            .ReplaceOneAsync(
                x => x.Id == commitment.Id,
                commitment);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Commitments
            .DeleteOneAsync(x => x.Id == id);
    }
}
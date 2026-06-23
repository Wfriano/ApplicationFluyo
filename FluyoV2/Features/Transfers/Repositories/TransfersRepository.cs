using FluyoV2.Features.Transfers.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Transfers.Repositories;

public class TransfersRepository
{
    private readonly MongoDbContext _context;

    public TransfersRepository(
        MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        Transfer transfer)
    {
        await _context.Transfers
            .InsertOneAsync(transfer);
    }

    public async Task<List<Transfer>>
        GetByUserAsync(string userId)
    {
        return await _context.Transfers
            .Find(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<Transfer?>
        GetByIdAsync(string id)
    {
        return await _context.Transfers
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }
}
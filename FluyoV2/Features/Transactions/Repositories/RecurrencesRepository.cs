using FluyoV2.Features.Transactions.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Transactions.Repositories;

public class RecurrencesRepository
{
    private readonly MongoDbContext _context;

    public RecurrencesRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Recurrence recurrence)
    {
        await _context.Recurrences.InsertOneAsync(recurrence);
    }

    public async Task<Recurrence?> GetByTransactionIdAsync(string transactionId)
    {
        return await _context.Recurrences
            .Find(x => x.TransactionId == transactionId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Recurrence>> GetDueRecurrencesAsync(DateTime upto)
    {
        return await _context.Recurrences
            .Find(x => x.NextDate <= upto)
            .ToListAsync();
    }

    public async Task<List<Recurrence>> GetByUserAsync(string userId)
    {
        return await _context.Recurrences
            .Find(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task UpdateAsync(Recurrence recurrence)
    {
        await _context.Recurrences
            .ReplaceOneAsync(x => x.Id == recurrence.Id, recurrence);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Recurrences.DeleteOneAsync(x => x.Id == id);
    }
}

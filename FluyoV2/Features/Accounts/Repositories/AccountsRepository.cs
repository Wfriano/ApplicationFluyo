using FluyoV2.Features.Accounts.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Accounts.Repositories;

public class AccountsRepository
{
    private readonly MongoDbContext _context;

    public AccountsRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Account account)
    {
        await _context.Accounts.InsertOneAsync(account);
    }

    public async Task<List<Account>> GetByUserIdAsync(string userId)
    {
        return await _context.Accounts
            .Find(x => x.UserId == userId && !x.IsArchived)
            .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(string id)
    {
        return await _context.Accounts
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Account?> GetByIdAsync(
        string accountId,
        string userId)
    {
        return await _context.Accounts
            .Find(x =>
                x.Id == accountId &&
                x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        await _context.Accounts.ReplaceOneAsync(
            x => x.Id == account.Id,
            account);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Accounts.DeleteOneAsync(
            x => x.Id == id);
    }

    public async Task<decimal> GetTotalBalanceAsync(
        string userId)
    {
        var accounts = await _context.Accounts
            .Find(x => x.UserId == userId &&
                       !x.IsArchived)
            .ToListAsync();

        return accounts.Sum(x => x.Balance);
    }

    public async Task UpdateBalanceAsync(
        string accountId,
        decimal newBalance)
    {
        var update = Builders<Account>.Update
            .Set(x => x.Balance, newBalance);

        await _context.Accounts.UpdateOneAsync(
            x => x.Id == accountId,
            update);
    }
}
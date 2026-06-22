using FluyoV2.Features.Transactions.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Transactions.Repositories;

public class TransactionsRepository
{
    private readonly MongoDbContext _context;

    public TransactionsRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Transaction transaction)
    {
        await _context.Transactions.InsertOneAsync(transaction);
    }

    public async Task<List<Transaction>> GetByUserAsync(
        string userId)
    {
        return await _context.Transactions
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.TransactionDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalIncomeAsync(
    string userId)
    {
        var transactions = await _context.Transactions
            .Find(x =>
                x.UserId == userId &&
                x.Type == "Income")
            .ToListAsync();

        return transactions.Sum(x => x.Amount);
    }

    public async Task<decimal> GetTotalExpensesAsync(
        string userId)
    {
        var transactions = await _context.Transactions
            .Find(x =>
                x.UserId == userId &&
                x.Type == "Expense")
            .ToListAsync();

        return transactions.Sum(x => x.Amount);
    }

    public async Task<int> GetTotalTransactionsAsync(
        string userId)
    {
        return (int)await _context.Transactions
            .CountDocumentsAsync(x =>
                x.UserId == userId);
    }
}
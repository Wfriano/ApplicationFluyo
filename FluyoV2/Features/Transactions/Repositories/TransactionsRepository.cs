using FluyoV2.Features.Transactions.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Linq;

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

    public async Task<List<Transaction>> GetByUserAsync(string userId, string? category, string? type, string? name)
    {
        var filter = Builders<Transaction>.Filter.Eq(x => x.UserId, userId);

        if (!string.IsNullOrWhiteSpace(category))
            filter = filter & Builders<Transaction>.Filter.Eq(x => x.Category, category);

        if (!string.IsNullOrWhiteSpace(type))
            filter = filter & Builders<Transaction>.Filter.Eq(x => x.Type, type);

        if (!string.IsNullOrWhiteSpace(name))
        {
            // split terms and build OR regex to match any related word (case-insensitive)
            var terms = name.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => Regex.Escape(t))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();

            if (terms.Length > 0)
            {
                var pattern = string.Join("|", terms);
                var regex = new BsonRegularExpression(pattern, "i");
                var descFilter = Builders<Transaction>.Filter.Regex(x => x.Description, regex);
                var catFilter = Builders<Transaction>.Filter.Regex(x => x.Category, regex);
                filter = filter & (descFilter | catFilter);
            }
        }

        return await _context.Transactions
            .Find(filter)
            .SortByDescending(x => x.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByUserAsync(string userId, string? category, string? type, string? name, int page, int pageSize)
    {
        var filter = Builders<Transaction>.Filter.Eq(x => x.UserId, userId);

        if (!string.IsNullOrWhiteSpace(category))
            filter = filter & Builders<Transaction>.Filter.Eq(x => x.Category, category);

        if (!string.IsNullOrWhiteSpace(type))
            filter = filter & Builders<Transaction>.Filter.Eq(x => x.Type, type);

        if (!string.IsNullOrWhiteSpace(name))
        {
            var terms = name.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => Regex.Escape(t))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();

            if (terms.Length > 0)
            {
                var pattern = string.Join("|", terms);
                var regex = new BsonRegularExpression(pattern, "i");
                var descFilter = Builders<Transaction>.Filter.Regex(x => x.Description, regex);
                var catFilter = Builders<Transaction>.Filter.Regex(x => x.Category, regex);
                filter = filter & (descFilter | catFilter);
            }
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;

        return await _context.Transactions
            .Find(filter)
            .SortByDescending(x => x.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountByUserAsync(string userId, string? category, string? type, string? name)
    {
        var filter = Builders<Transaction>.Filter.Eq(x => x.UserId, userId);

        if (!string.IsNullOrWhiteSpace(category))
            filter = filter & Builders<Transaction>.Filter.Eq(x => x.Category, category);

        if (!string.IsNullOrWhiteSpace(type))
            filter = filter & Builders<Transaction>.Filter.Eq(x => x.Type, type);

        if (!string.IsNullOrWhiteSpace(name))
        {
            var terms = name.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => Regex.Escape(t))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();

            if (terms.Length > 0)
            {
                var pattern = string.Join("|", terms);
                var regex = new BsonRegularExpression(pattern, "i");
                var descFilter = Builders<Transaction>.Filter.Regex(x => x.Description, regex);
                var catFilter = Builders<Transaction>.Filter.Regex(x => x.Category, regex);
                filter = filter & (descFilter | catFilter);
            }
        }

        return (int)await _context.Transactions.CountDocumentsAsync(filter);
    }

    public async Task<Transaction?> GetByIdAsync(string id)
    {
        return await _context.Transactions
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        await _context.Transactions
            .ReplaceOneAsync(x => x.Id == transaction.Id, transaction);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Transactions.DeleteOneAsync(x => x.Id == id);
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

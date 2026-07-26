using FluyoV2.Features.Accounts.Models;
using FluyoV2.Features.Auth.Models;
using FluyoV2.Features.Commitments.Models;
using FluyoV2.Features.Goals.Models;
using FluyoV2.Features.Transactions.Models;
using FluyoV2.Features.Transfers.Models;
using FluyoV2.Settings;
using MongoDB.Driver;

namespace FluyoV2.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(MongoDbSettings settings)
    {
        _settings = settings;

        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>(
            _settings.UsersCollectionName);

    public IMongoCollection<Account> Accounts =>
        _database.GetCollection<Account>(
            _settings.AccountsCollectionName);

    public IMongoCollection<Transaction> Transactions =>
        _database.GetCollection<Transaction>(
            _settings.TransactionsCollectionName);

    public IMongoCollection<Goal> Goals =>
        _database.GetCollection<Goal>(
            _settings.GoalsCollectionName);

    public IMongoCollection<Commitment> Commitments =>
        _database.GetCollection<Commitment>("Commitments");

    public IMongoCollection<Transfer> Transfers =>
    _database.GetCollection<Transfer>("Transfers");
}
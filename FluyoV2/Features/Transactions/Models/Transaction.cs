using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FluyoV2.Features.Transactions.Models;

public class Transaction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    // Income | Expense
    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }
        = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}
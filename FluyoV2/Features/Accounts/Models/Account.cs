using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FluyoV2.Features.Accounts.Models;

public class Account
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    // Cash, Bank, Wallet, CreditCard
    public string Type { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "COP";

    // Optional color for the account icon (mapped from incoming XML/JSON)
    public string IconColor { get; set; } = string.Empty;

    public bool IsArchived { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FluyoV2.Features.Transactions.Models;

public enum Frequency
{
    Mensual,
    Quincenal,
    Semanal
}

public class Recurrence
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string TransactionId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public Frequency Frequency { get; set; }

    public DateTime NextDate { get; set; }

    // If null => Indefinido
    public DateTime? EndDate { get; set; }

    // The monetary amount that will be created by the job (separate movement)
    public decimal Amount { get; set; }

    // Income | Expense
    public string Type { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Account where the scheduled movement will be applied
    [BsonRepresentation(BsonType.ObjectId)]
    public string AccountId { get; set; } = string.Empty;

    // Optional secondary account (e.g., other account control in UI)
    [BsonRepresentation(BsonType.ObjectId)]
    public string? OtherAccountId { get; set; }

    // Payment status / flag shown in UI
    public bool IsPaid { get; set; }

    // Optional note or user-entered extra text
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

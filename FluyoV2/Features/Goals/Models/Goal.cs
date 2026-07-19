using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FluyoV2.Features.Goals.Models;

public class Goal
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public bool IsCompleted { get; set; }
    public string Image { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}
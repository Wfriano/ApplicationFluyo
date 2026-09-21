using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FluyoV2.Features.Notifications.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string SourceId { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }

    public bool IsRead { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public string DedupKey { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? SentAt { get; set; }

    public string Status { get; set; } = "pending";

    public string? ErrorMessage { get; set; }
}

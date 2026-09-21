using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FluyoV2.Features.Notifications.Models;

public class NotificationDevice
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    public string InstallationId { get; set; } = string.Empty;

    public string ExpoPushToken { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string TimeZone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UnregisteredAt { get; set; }
}

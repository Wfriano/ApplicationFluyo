namespace FluyoV2.Features.Notifications.Models;

public class Notification
{
    public string Id { get; set; }
        = Guid.NewGuid().ToString();

    public string UserId { get; set; }
        = string.Empty;

    public string Title { get; set; }
        = string.Empty;

    public string Message { get; set; }
        = string.Empty;

    public string SourceType { get; set; }
        = string.Empty;

    public string SourceId { get; set; }
        = string.Empty;

    public DateTime PaymentDate { get; set; }

    public bool IsRead { get; set; }
        = false;

    public bool IsDeleted { get; set; }
        = false;

    // Marker used to avoid duplicate notifications for the same source/day.
    public string DedupKey { get; set; }
        = string.Empty;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }
}

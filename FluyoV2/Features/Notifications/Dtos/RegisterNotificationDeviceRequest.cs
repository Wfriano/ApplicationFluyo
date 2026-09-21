namespace FluyoV2.Features.Notifications.Dtos;

public class RegisterNotificationDeviceRequest
{
    public string InstallationId { get; set; } = string.Empty;

    public string ExpoPushToken { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string TimeZone { get; set; } = string.Empty;
}

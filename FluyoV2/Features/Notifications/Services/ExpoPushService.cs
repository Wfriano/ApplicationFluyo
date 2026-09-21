using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Notifications.Services;

public class ExpoPushService
{
    private const string ExpoPushEndpoint = "https://exp.host/--/api/v2/push/send";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExpoPushService> _logger;

    public ExpoPushService(IHttpClientFactory httpClientFactory, ILogger<ExpoPushService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string expoPushToken, string title, string message, object? data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(expoPushToken))
            return false;

        var client = _httpClientFactory.CreateClient();
        var payload = new
        {
            to = expoPushToken,
            sound = "default",
            title,
            body = message,
            data
        };

        using var response = await client.PostAsJsonAsync(ExpoPushEndpoint, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Expo push failed. Token: {Token}, StatusCode: {StatusCode}, Response: {Response}", expoPushToken, response.StatusCode, responseText);
            return false;
        }

        return true;
    }
}

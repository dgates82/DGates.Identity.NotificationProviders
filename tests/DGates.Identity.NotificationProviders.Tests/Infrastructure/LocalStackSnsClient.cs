using System.Text.Json;

namespace DGates.Identity.NotificationProviders.Tests.Infrastructure;

// Thin wrapper over LocalStack's SNS SMS introspection API (docker-compose's localstack
// service, edge port 4566). SNS SMS has no real delivery to observe even against LocalStack,
// so this reads back the captured publish requests instead of asserting on a mock inbox.
public class LocalStackSnsClient
{
    private readonly HttpClient _http;

    public LocalStackSnsClient(string baseUrl = "http://localhost:4566")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task DeleteAllMessagesAsync()
    {
        var response = await _http.DeleteAsync("/_aws/sns/sms-messages");
        response.EnsureSuccessStatusCode();
    }

    public async Task<JsonElement?> FindMessageToAsync(string phoneNumber, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            var response = await _http.GetAsync("/_aws/sns/sms-messages");
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.GetProperty("sms_messages").TryGetProperty(phoneNumber, out var messages)
                && messages.GetArrayLength() > 0)
            {
                return messages[0].Clone();
            }

            if (DateTime.UtcNow >= deadline)
            {
                return null;
            }

            await Task.Delay(200);
        }
    }
}

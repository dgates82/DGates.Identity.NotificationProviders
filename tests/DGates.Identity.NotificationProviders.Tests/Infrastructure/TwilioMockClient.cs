using System.Text.Json;

namespace DGates.Identity.NotificationProviders.Tests.Infrastructure;

// Thin wrapper over the twillio-sms-mock REST API (docker-compose's twiliomock service,
// port 3030) so tests can assert an SMS was actually sent and extract its body - unlike
// Mailpit, the list endpoint already includes the full message body, no second call needed.
public class TwilioMockClient
{
    private readonly HttpClient _http;

    public TwilioMockClient(string baseUrl = "http://localhost:3030")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task DeleteAllMessagesAsync()
    {
        var response = await _http.DeleteAsync("/api/messages");
        response.EnsureSuccessStatusCode();
    }

    public async Task<JsonElement?> FindMessageToAsync(string phoneNumber, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            var response = await _http.GetAsync("/api/messages");
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            foreach (var message in doc.RootElement.EnumerateArray())
            {
                var to = message.GetProperty("to").GetString() ?? "";
                if (to.Contains(phoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    return message.Clone();
                }
            }

            if (DateTime.UtcNow >= deadline)
            {
                return null;
            }

            await Task.Delay(200);
        }
    }
}

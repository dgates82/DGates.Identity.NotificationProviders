using System.Text.Json;

namespace DGates.Identity.NotificationProviders.Tests.Infrastructure;

// Thin wrapper over the Mailpit v1 HTTP API (docker-compose's mailpit service, web UI
// port 8025) so tests can assert an email was actually sent rather than just trusting
// IEmailSender didn't throw.
public class MailpitClient
{
    private readonly HttpClient _http;

    public MailpitClient(string baseUrl = "http://localhost:8025")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task DeleteAllMessagesAsync()
    {
        var response = await _http.DeleteAsync("/api/v1/messages");
        response.EnsureSuccessStatusCode();
    }

    public async Task<JsonElement?> FindMessageToAsync(string emailAddress, string subjectContains, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            var response = await _http.GetAsync("/api/v1/messages?limit=50");
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("messages", out var messages))
            {
                foreach (var message in messages.EnumerateArray())
                {
                    var subject = message.GetProperty("Subject").GetString() ?? "";
                    if (!subject.Contains(subjectContains, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var to = message.GetProperty("To").EnumerateArray()
                        .Select(t => t.GetProperty("Address").GetString());
                    if (to.Contains(emailAddress, StringComparer.OrdinalIgnoreCase))
                    {
                        return message.Clone();
                    }
                }
            }

            if (DateTime.UtcNow >= deadline)
            {
                return null;
            }

            await Task.Delay(200);
        }
    }

    // The list endpoint only returns headers (Subject/To) - fetching a single message
    // is a separate call that includes the actual body, needed to extract a 2FA code.
    public async Task<string> GetMessageTextAsync(string messageId)
    {
        var response = await _http.GetAsync($"/api/v1/message/{messageId}");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("Text").GetString() ?? "";
    }
}

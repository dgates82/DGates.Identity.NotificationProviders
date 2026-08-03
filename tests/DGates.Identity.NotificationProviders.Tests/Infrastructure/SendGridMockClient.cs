using System.Text.Json;

namespace DGates.Identity.NotificationProviders.Tests.Infrastructure;

// Thin wrapper over the SendGrid mock REST API (docker-compose's sendgridmock service,
// port 3040) so tests can assert an email was actually sent rather than just trusting
// IEmailSender didn't throw.
public class SendGridMockClient
{
    private readonly HttpClient _http;

    public SendGridMockClient(string baseUrl = "http://localhost:3040")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task DeleteAllMessagesAsync()
    {
        var response = await _http.DeleteAsync("/api/messages");
        response.EnsureSuccessStatusCode();
    }

    public async Task<JsonElement?> FindMessageToAsync(string emailAddress, string subjectContains, TimeSpan timeout)
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
                var subject = message.GetProperty("subject").GetString() ?? "";
                if (to.Equals(emailAddress, StringComparison.OrdinalIgnoreCase)
                    && subject.Contains(subjectContains, StringComparison.OrdinalIgnoreCase))
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

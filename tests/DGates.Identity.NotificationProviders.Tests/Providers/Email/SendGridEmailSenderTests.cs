using DGates.Identity.NotificationProviders.Options.Email;
using DGates.Identity.NotificationProviders.Providers.Email;
using DGates.Identity.NotificationProviders.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Email;

[Trait("Category", "Integration")]
public class SendGridEmailSenderTests
{
    private readonly SendGridEmailSender _sender;
    private readonly SendGridMockClient _sendGridMock = new();

    public SendGridEmailSenderTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new SendGridEmailOptions
        {
            ApiKey = "test-api-key",
            FromAddress = "sender@notification-providers.test",
            FromName = "Notification Providers Test",
            BaseUrlOverride = "http://localhost:3040"
        });

        _sender = new SendGridEmailSender(NullLogger<SendGridEmailSender>.Instance, options);
    }

    [Fact]
    public async Task SendEmailAsync_DeliversMessageToMock()
    {
        await _sendGridMock.DeleteAllMessagesAsync();
        var toEmail = $"{Guid.NewGuid()}@notification-providers.test";
        var subject = $"Test Subject {Guid.NewGuid()}";

        await _sender.SendEmailAsync(toEmail, subject, "<p>Hello from SendGridEmailSenderTests</p>");

        var message = await _sendGridMock.FindMessageToAsync(toEmail, subject, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
    }

    [Fact]
    public async Task SendEmailAsync_WithOverrideRecipient_RedirectsAndAppendsOriginalRecipientToSubject()
    {
        await _sendGridMock.DeleteAllMessagesAsync();
        const string overrideRecipient = "override@notification-providers.test";
        var sender = new OverrideRecipientEmailSender(_sender, overrideRecipient);
        var originalRecipient = $"{Guid.NewGuid()}@notification-providers.test";
        var subject = $"Test Subject {Guid.NewGuid()}";

        await sender.SendEmailAsync(originalRecipient, subject, "<p>Hello</p>");

        var message = await _sendGridMock.FindMessageToAsync(overrideRecipient, subject, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
    }
}

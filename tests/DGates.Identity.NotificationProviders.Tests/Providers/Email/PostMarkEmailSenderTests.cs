using DGates.Identity.NotificationProviders.Options.Email;
using DGates.Identity.NotificationProviders.Providers.Email;
using DGates.Identity.NotificationProviders.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Email;

[Trait("Category", "Integration")]
public class PostMarkEmailSenderTests
{
    private readonly PostMarkEmailSender _sender;
    private readonly PostmarkMockClient _postmarkMock = new();

    public PostMarkEmailSenderTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new PostMarkEmailOptions
        {
            ApiKey = "test-api-key",
            FromAddress = "sender@notification-providers.test",
            BaseUrlOverride = "http://localhost:3050"
        });

        _sender = new PostMarkEmailSender(NullLogger<PostMarkEmailSender>.Instance, options);
    }

    [Fact]
    public async Task SendEmailAsync_DeliversMessageToMock()
    {
        await _postmarkMock.DeleteAllMessagesAsync();
        var toEmail = $"{Guid.NewGuid()}@notification-providers.test";
        var subject = $"Test Subject {Guid.NewGuid()}";

        await _sender.SendEmailAsync(toEmail, subject, "<p>Hello from PostMarkEmailSenderTests</p>");

        var message = await _postmarkMock.FindMessageToAsync(toEmail, subject, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
    }

    [Fact]
    public async Task SendEmailAsync_WithOverrideRecipient_RedirectsAndAppendsOriginalRecipientToSubject()
    {
        await _postmarkMock.DeleteAllMessagesAsync();
        const string overrideRecipient = "override@notification-providers.test";
        var sender = new OverrideRecipientEmailSender(_sender, overrideRecipient);
        var originalRecipient = $"{Guid.NewGuid()}@notification-providers.test";
        var subject = $"Test Subject {Guid.NewGuid()}";

        await sender.SendEmailAsync(originalRecipient, subject, "<p>Hello</p>");

        var message = await _postmarkMock.FindMessageToAsync(overrideRecipient, subject, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
    }
}

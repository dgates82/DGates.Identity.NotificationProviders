using DGates.Identity.NotificationProviders.Options.Email;
using DGates.Identity.NotificationProviders.Providers.Email;
using DGates.Identity.NotificationProviders.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Email;

[Trait("Category", "Integration")]
public class SmtpEmailSenderTests
{
    private readonly SmtpEmailSender _sender;
    private readonly MailpitClient _mailpit = new();

    public SmtpEmailSenderTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new SmtpEmailOptions
        {
            Host = "localhost",
            Port = "1025",
            FromAddress = "sender@notification-providers.test",
            EnableSsl = false
        });

        _sender = new SmtpEmailSender(NullLogger<SmtpEmailSender>.Instance, options);
    }

    [Fact]
    public async Task SendEmailAsync_DeliversMessageToMailpit()
    {
        await _mailpit.DeleteAllMessagesAsync();
        var toEmail = $"{Guid.NewGuid()}@notification-providers.test";
        var subject = $"Test Subject {Guid.NewGuid()}";

        await _sender.SendEmailAsync(toEmail, subject, "<p>Hello from SmtpEmailSenderTests</p>");

        var message = await _mailpit.FindMessageToAsync(toEmail, subject, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
    }

    [Fact]
    public async Task SendEmailAsync_WithOverrideRecipient_RedirectsAndAppendsOriginalRecipientToSubject()
    {
        await _mailpit.DeleteAllMessagesAsync();
        const string overrideRecipient = "override@notification-providers.test";
        var options = Microsoft.Extensions.Options.Options.Create(new SmtpEmailOptions
        {
            Host = "localhost",
            Port = "1025",
            FromAddress = "sender@notification-providers.test",
            EnableSsl = false,
            OverrideRecipient = overrideRecipient
        });
        var sender = new SmtpEmailSender(NullLogger<SmtpEmailSender>.Instance, options);
        var originalRecipient = $"{Guid.NewGuid()}@notification-providers.test";
        var subject = $"Test Subject {Guid.NewGuid()}";

        await sender.SendEmailAsync(originalRecipient, subject, "<p>Hello</p>");

        var message = await _mailpit.FindMessageToAsync(overrideRecipient, subject, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
    }
}

using DGates.Identity.NotificationProviders.Providers.Email;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Email;

public class OverrideRecipientEmailSenderTests
{
    private class FakeEmailSender : IEmailSender
    {
        public string? ToEmail;
        public string? Subject;
        public string? Message;

        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            ToEmail = toEmail;
            Subject = subject;
            Message = message;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyOverrideRecipient_PassesThroughUnchanged()
    {
        var inner = new FakeEmailSender();
        var sender = new OverrideRecipientEmailSender(inner, "");

        await sender.SendEmailAsync("real@example.com", "Subject", "Message");

        Assert.Equal("real@example.com", inner.ToEmail);
        Assert.Equal("Subject", inner.Subject);
        Assert.Equal("Message", inner.Message);
    }

    [Fact]
    public async Task SendEmailAsync_WithOverrideRecipient_RedirectsAndAppendsOriginalRecipientToSubject()
    {
        var inner = new FakeEmailSender();
        var sender = new OverrideRecipientEmailSender(inner, "override@example.com");

        await sender.SendEmailAsync("real@example.com", "Subject", "Message");

        Assert.Equal("override@example.com", inner.ToEmail);
        Assert.Equal("Subject - Original Recipient: real@example.com", inner.Subject);
        Assert.Equal("Message", inner.Message);
    }
}

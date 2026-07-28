using DGates.Identity.NotificationProviders.Abstractions;
using DGates.Identity.NotificationProviders.Providers.Sms;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Sms;

public class OverrideRecipientSmsSenderTests
{
    private class FakeSmsSender : ISmsSender
    {
        public string? Number;
        public string? Message;

        public Task<string> SendSmsAsync(string number, string message, CancellationToken cancellationToken = default)
        {
            Number = number;
            Message = message;
            return Task.FromResult("fake-message-id");
        }
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyOverrideRecipient_PassesThroughUnchanged()
    {
        var inner = new FakeSmsSender();
        var sender = new OverrideRecipientSmsSender(inner, "");

        var messageId = await sender.SendSmsAsync("+15555550100", "Message");

        Assert.Equal("+15555550100", inner.Number);
        Assert.Equal("Message", inner.Message);
        Assert.Equal("fake-message-id", messageId);
    }

    [Fact]
    public async Task SendSmsAsync_WithOverrideRecipient_RedirectsAndAppendsOriginalNumberToBody()
    {
        var inner = new FakeSmsSender();
        var sender = new OverrideRecipientSmsSender(inner, "+15555550199");

        await sender.SendSmsAsync("+15555550100", "Message");

        Assert.Equal("+15555550199", inner.Number);
        Assert.Equal("Message - Original Recipient: +15555550100", inner.Message);
    }
}

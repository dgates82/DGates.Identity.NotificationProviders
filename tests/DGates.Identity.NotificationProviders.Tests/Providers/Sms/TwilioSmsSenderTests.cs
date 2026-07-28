using DGates.Identity.NotificationProviders.Options.Sms;
using DGates.Identity.NotificationProviders.Providers.Sms;
using DGates.Identity.NotificationProviders.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Sms;

[Trait("Category", "Integration")]
public class TwilioSmsSenderTests
{
    private readonly TwilioSmsSender _sender;
    private readonly SmsMockClient _smsMock = new();

    public TwilioSmsSenderTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new TwilioSmsOptions
        {
            AccountSid = "ACtest",
            AuthToken = "test",
            FromNumber = "+15555550100",
            BaseUrlOverride = "http://localhost:3030"
        });

        _sender = new TwilioSmsSender(options, NullLogger<TwilioSmsSender>.Instance);
    }

    [Fact]
    public async Task SendSmsAsync_DeliversMessageToMock()
    {
        await _smsMock.DeleteAllMessagesAsync();
        var toNumber = "+1555555" + Random.Shared.Next(1000, 9999);
        var body = $"Test message {Guid.NewGuid()}";

        await _sender.SendSmsAsync(toNumber, body);

        var message = await _smsMock.FindMessageToAsync(toNumber, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
        Assert.Equal(body, message!.Value.GetProperty("body").GetString());
    }

    [Fact]
    public async Task SendSmsAsync_WithOverrideRecipient_RedirectsAndAppendsOriginalNumberToBody()
    {
        await _smsMock.DeleteAllMessagesAsync();
        const string overrideRecipient = "+15555550199";
        var sender = new OverrideRecipientSmsSender(_sender, overrideRecipient);
        var originalNumber = "+1555555" + Random.Shared.Next(1000, 9999);

        await sender.SendSmsAsync(originalNumber, "Test message");

        var message = await _smsMock.FindMessageToAsync(overrideRecipient, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
        Assert.Contains(originalNumber, message!.Value.GetProperty("body").GetString());
    }
}

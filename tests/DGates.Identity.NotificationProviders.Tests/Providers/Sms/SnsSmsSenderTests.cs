using DGates.Identity.NotificationProviders.Options.Sms;
using DGates.Identity.NotificationProviders.Providers.Sms;
using DGates.Identity.NotificationProviders.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Sms;

[Trait("Category", "Integration")]
public class SnsSmsSenderTests
{
    private readonly SnsSmsSender _sender;
    private readonly LocalStackSnsClient _sns = new();

    public SnsSmsSenderTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new SnsSmsOptions
        {
            Region = "us-east-1",
            AccessKey = "test",
            SecretKey = "test",
            ServiceUrlOverride = "http://localhost:4566"
        });

        _sender = new SnsSmsSender(options, NullLogger<SnsSmsSender>.Instance);
    }

    [Fact]
    public async Task SendSmsAsync_DeliversMessageToLocalStack()
    {
        await _sns.DeleteAllMessagesAsync();
        var toNumber = "+1555555" + Random.Shared.Next(1000, 9999);
        var body = $"Test message {Guid.NewGuid()}";

        await _sender.SendSmsAsync(toNumber, body);

        var message = await _sns.FindMessageToAsync(toNumber, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
        Assert.Equal(body, message!.Value.GetProperty("Message").GetString());
    }

    [Fact]
    public async Task SendSmsAsync_WithOverrideRecipient_RedirectsAndAppendsOriginalNumberToBody()
    {
        await _sns.DeleteAllMessagesAsync();
        const string overrideRecipient = "+15555550199";
        var sender = new OverrideRecipientSmsSender(_sender, overrideRecipient);
        var originalNumber = "+1555555" + Random.Shared.Next(1000, 9999);

        await sender.SendSmsAsync(originalNumber, "Test message");

        var message = await _sns.FindMessageToAsync(overrideRecipient, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
        Assert.Contains(originalNumber, message!.Value.GetProperty("Message").GetString());
    }

    [Fact]
    public async Task SendSmsAsync_WithSenderId_SetsMessageAttribute()
    {
        await _sns.DeleteAllMessagesAsync();
        var options = Microsoft.Extensions.Options.Options.Create(new SnsSmsOptions
        {
            Region = "us-east-1",
            AccessKey = "test",
            SecretKey = "test",
            ServiceUrlOverride = "http://localhost:4566",
            SenderId = "TESTSENDER"
        });
        var sender = new SnsSmsSender(options, NullLogger<SnsSmsSender>.Instance);
        var toNumber = "+1555555" + Random.Shared.Next(1000, 9999);

        await sender.SendSmsAsync(toNumber, "Test message");

        var message = await _sns.FindMessageToAsync(toNumber, TimeSpan.FromSeconds(10));
        Assert.NotNull(message);
        var senderId = message!.Value.GetProperty("MessageAttributes").GetProperty("AWS.SNS.SMS.SenderID").GetProperty("StringValue").GetString();
        Assert.Equal("TESTSENDER", senderId);
    }
}

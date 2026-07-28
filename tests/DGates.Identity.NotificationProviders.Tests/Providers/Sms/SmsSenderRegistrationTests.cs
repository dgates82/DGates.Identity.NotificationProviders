using DGates.Identity.NotificationProviders.Abstractions;
using DGates.Identity.NotificationProviders.Providers.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Sms;

public class SmsSenderRegistrationTests
{
    private static IConfiguration BuildConfiguration(string section, Dictionary<string, string?> values)
    {
        var prefixed = values.ToDictionary(kv => $"{section}:{kv.Key}", kv => kv.Value);
        return new ConfigurationBuilder().AddInMemoryCollection(prefixed).Build();
    }

    [Fact]
    public void AddTwilioSmsSender_ResolvesAsOverrideRecipientSmsSender()
    {
        var configuration = BuildConfiguration("TwilioSmsConfigs", new()
        {
            ["AccountSid"] = "ACtest",
            ["AuthToken"] = "test",
            ["FromNumber"] = "+15555550100"
        });
        var services = new ServiceCollection().AddLogging();
        services.AddTwilioSmsSender(configuration);

        var sender = services.BuildServiceProvider().GetRequiredService<ISmsSender>();

        Assert.IsType<OverrideRecipientSmsSender>(sender);
    }

    [Fact]
    public void AddSnsSmsSender_ResolvesAsOverrideRecipientSmsSender()
    {
        var configuration = BuildConfiguration("SnsSmsConfigs", new()
        {
            ["Region"] = "us-east-1"
        });
        var services = new ServiceCollection().AddLogging();
        services.AddSnsSmsSender(configuration);

        var sender = services.BuildServiceProvider().GetRequiredService<ISmsSender>();

        Assert.IsType<OverrideRecipientSmsSender>(sender);
    }
}

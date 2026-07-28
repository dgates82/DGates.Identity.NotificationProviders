using DGates.Identity.NotificationProviders.Providers.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DGates.Identity.NotificationProviders.Tests.Providers.Email;

public class EmailSenderRegistrationTests
{
    private static IConfiguration BuildConfiguration(string section, Dictionary<string, string?> values)
    {
        var prefixed = values.ToDictionary(kv => $"{section}:{kv.Key}", kv => kv.Value);
        return new ConfigurationBuilder().AddInMemoryCollection(prefixed).Build();
    }

    [Fact]
    public void AddSmtpEmailSender_ResolvesAsOverrideRecipientEmailSender()
    {
        var configuration = BuildConfiguration("SmtpEmailConfigs", new()
        {
            ["Host"] = "localhost",
            ["Port"] = "1025",
            ["FromAddress"] = "sender@notification-providers.test"
        });
        var services = new ServiceCollection().AddLogging();
        services.AddSmtpEmailSender(configuration);

        var sender = services.BuildServiceProvider().GetRequiredService<IEmailSender>();

        Assert.IsType<OverrideRecipientEmailSender>(sender);
    }

    [Fact]
    public void AddSendGridEmailSender_ResolvesAsOverrideRecipientEmailSender()
    {
        var configuration = BuildConfiguration("SendGridEmailConfigs", new()
        {
            ["ApiKey"] = "test-api-key",
            ["FromAddress"] = "sender@notification-providers.test",
            ["FromName"] = "Test Sender"
        });
        var services = new ServiceCollection().AddLogging();
        services.AddSendGridEmailSender(configuration);

        var sender = services.BuildServiceProvider().GetRequiredService<IEmailSender>();

        Assert.IsType<OverrideRecipientEmailSender>(sender);
    }

    [Fact]
    public void AddPostMarkEmailSender_ResolvesAsOverrideRecipientEmailSender()
    {
        var configuration = BuildConfiguration("PostMarkEmailConfigs", new()
        {
            ["ApiKey"] = "test-api-key",
            ["FromAddress"] = "sender@notification-providers.test"
        });
        var services = new ServiceCollection().AddLogging();
        services.AddPostMarkEmailSender(configuration);

        var sender = services.BuildServiceProvider().GetRequiredService<IEmailSender>();

        Assert.IsType<OverrideRecipientEmailSender>(sender);
    }
}

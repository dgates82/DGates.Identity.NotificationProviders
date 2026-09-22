using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using DGates.Identity.NotificationProviders.Abstractions;
using DGates.Identity.NotificationProviders.ExtensionMethods;
using DGates.Identity.NotificationProviders.Options.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DGates.Identity.NotificationProviders.Providers.Sms;

/// <summary>
/// <see cref="ISmsSender"/> implementation, sending SMS via AWS SNS.
/// </summary>
public class SnsSmsSender : ISmsSender
{
    private readonly IOptions<SnsSmsOptions> _options;
    private readonly ILogger<SnsSmsSender> _logger;

    /// <summary>Creates the sender with its injected SNS configuration options and logger.</summary>
    public SnsSmsSender(IOptions<SnsSmsOptions> options, ILogger<SnsSmsSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>Sends an SMS via AWS SNS, returning the SNS message ID.</summary>
    public async Task<string> SendSmsAsync(string number, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SendSmsAsync | messageLength: {MessageLength}", message.Length);

        using var client = CreateClient();

        var request = new PublishRequest
        {
            PhoneNumber = number,
            Message = message,
            MessageAttributes = BuildMessageAttributes()
        };

        var response = await client.PublishAsync(request, cancellationToken);

        _logger.LogInformation("Message sent to {Number}, MessageId: {MessageId}", number.MaskPhone(), response.MessageId);

        return response.MessageId;
    }

    private IAmazonSimpleNotificationService CreateClient()
    {
        var config = new AmazonSimpleNotificationServiceConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Value.Region)
        };

        if (!string.IsNullOrEmpty(_options.Value.ServiceUrlOverride))
        {
            config.ServiceURL = _options.Value.ServiceUrlOverride;
        }

        if (!string.IsNullOrEmpty(_options.Value.AccessKey))
        {
            return new AmazonSimpleNotificationServiceClient(_options.Value.AccessKey, _options.Value.SecretKey, config);
        }

        return new AmazonSimpleNotificationServiceClient(config);
    }

    private Dictionary<string, MessageAttributeValue> BuildMessageAttributes()
    {
        var attributes = new Dictionary<string, MessageAttributeValue>();

        if (!string.IsNullOrEmpty(_options.Value.SenderId))
        {
            attributes["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue { DataType = "String", StringValue = _options.Value.SenderId };
        }

        if (!string.IsNullOrEmpty(_options.Value.SmsType))
        {
            attributes["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue { DataType = "String", StringValue = _options.Value.SmsType };
        }

        return attributes;
    }
}

/// <summary>Registers <see cref="SnsSmsSender"/> as the <see cref="ISmsSender"/> implementation.</summary>
public static class SnsSmsSenderServiceCollectionExtensions
{
    /// <summary>Binds <see cref="SnsSmsOptions"/> from configuration and registers <see cref="SnsSmsSender"/>, wrapped with <see cref="OverrideRecipientSmsSender"/>.</summary>
    public static IServiceCollection AddSnsSmsSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SnsSmsOptions>(configuration.GetSection(SnsSmsOptions.ConfigSection));
        services.AddTransient<ISmsSender>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SnsSmsOptions>>();
            var inner = new SnsSmsSender(options, sp.GetRequiredService<ILogger<SnsSmsSender>>());
            return new OverrideRecipientSmsSender(inner, options.Value.OverrideRecipient);
        });
        return services;
    }
}

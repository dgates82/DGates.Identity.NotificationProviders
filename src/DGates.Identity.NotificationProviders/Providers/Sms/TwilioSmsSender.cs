using DGates.Identity.NotificationProviders.Abstractions;
using DGates.Identity.NotificationProviders.Options.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace DGates.Identity.NotificationProviders.Providers.Sms;

/// <summary>
/// Default <see cref="ISmsSender"/> implementation, sending SMS via Twilio.
/// Used for the SMS-based 2FA delivery method.
/// </summary>
public class TwilioSmsSender : ISmsSender
{
    private readonly IOptions<TwilioSmsOptions> _options;
    private readonly ILogger<TwilioSmsSender> _logger;

    /// <summary>Creates the sender with its injected Twilio configuration options and logger.</summary>
    public TwilioSmsSender(IOptions<TwilioSmsOptions> options, ILogger<TwilioSmsSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Sends an SMS via Twilio. If
    /// <see cref="TwilioSmsOptions.OverrideRecipient"/> is configured, the
    /// message is redirected there instead, with the original recipient
    /// appended to the message body.
    /// </summary>
    public Task SendSmsAsync(string number, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug($"SendSmsAsync | number: {number} | message: {message}");

        if (!string.IsNullOrEmpty(_options.Value.BaseUrlOverride))
        {
            var mockClient = new TwilioRestClient(_options.Value.AccountSid, _options.Value.AuthToken,
                httpClient: new TwilioMockRedirectHttpClient(_options.Value.BaseUrlOverride));
            TwilioClient.SetRestClient(mockClient);
        }
        else
        {
            TwilioClient.Init(_options.Value.AccountSid, _options.Value.AuthToken);
        }

        // Set number based on override 
        var finalNumber = !string.IsNullOrEmpty(_options.Value.OverrideRecipient) ? _options.Value.OverrideRecipient : number;
        
        _logger.LogDebug($"SendSmsAsync | Final Number: {finalNumber}");
        
        // Include original number in message if override is used
        if (!string.IsNullOrEmpty(_options.Value.OverrideRecipient))
        {
            message += $" - Original Recipient: {number}";
        }
        
        _logger.LogDebug($"SendSmsAsync | Final Message: {message}");
        
        // The Twilio SDK has no CancellationToken overload, so this can only abandon the
        // wait on cancellation, not actually cancel the outbound request.
        return MessageResource.CreateAsync(
            body: message,
            from: new Twilio.Types.PhoneNumber(_options.Value.FromNumber),
            to: new Twilio.Types.PhoneNumber(finalNumber)
        ).WaitAsync(cancellationToken);

    }

}

/// <summary>Registers <see cref="TwilioSmsSender"/> as the <see cref="ISmsSender"/> implementation.</summary>
public static class TwilioSmsSenderServiceCollectionExtensions
{
    /// <summary>Binds <see cref="TwilioSmsOptions"/> from configuration and registers <see cref="TwilioSmsSender"/>.</summary>
    public static IServiceCollection AddTwilioSmsSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TwilioSmsOptions>(configuration.GetSection(TwilioSmsOptions.ConfigSection));
        services.AddTransient<ISmsSender, TwilioSmsSender>();
        return services;
    }
}
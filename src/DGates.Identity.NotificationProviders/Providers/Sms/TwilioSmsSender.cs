using DGates.Identity.NotificationProviders.Abstractions;
using DGates.Identity.NotificationProviders.Options.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace DGates.Identity.NotificationProviders.Providers.Sms;

/// <summary>
/// <see cref="ISmsSender"/> implementation, sending SMS via Twilio.
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

    /// <summary>Sends an SMS via Twilio, returning the Twilio message SID.</summary>
    public async Task<string> SendSmsAsync(string number, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug($"SendSmsAsync | number: {number} | message: {message}");

        // The Twilio SDK has no CancellationToken overload, and MessageResource.CreateAsync
        // has no way to cancel a request already in flight - this only avoids starting one
        // if cancellation was requested beforehand.
        cancellationToken.ThrowIfCancellationRequested();

        // Built per-call and passed explicitly (rather than TwilioClient.Init/SetRestClient,
        // which mutate process-wide static state) so concurrent calls with different options
        // - e.g. one with BaseUrlOverride set, one without - can't race on a shared client.
        ITwilioRestClient client = !string.IsNullOrEmpty(_options.Value.BaseUrlOverride)
            ? new TwilioRestClient(_options.Value.AccountSid, _options.Value.AuthToken,
                httpClient: new TwilioMockRedirectHttpClient(_options.Value.BaseUrlOverride))
            : new TwilioRestClient(_options.Value.AccountSid, _options.Value.AuthToken);

        var result = await MessageResource.CreateAsync(
            body: message,
            from: new Twilio.Types.PhoneNumber(_options.Value.FromNumber),
            to: new Twilio.Types.PhoneNumber(number),
            client: client
        );

        _logger.LogInformation($"Message sent to {number}, Sid: {result.Sid}");

        return result.Sid;
    }

}

/// <summary>Registers <see cref="TwilioSmsSender"/> as the <see cref="ISmsSender"/> implementation.</summary>
public static class TwilioSmsSenderServiceCollectionExtensions
{
    /// <summary>Binds <see cref="TwilioSmsOptions"/> from configuration and registers <see cref="TwilioSmsSender"/>, wrapped with <see cref="OverrideRecipientSmsSender"/>.</summary>
    public static IServiceCollection AddTwilioSmsSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TwilioSmsOptions>(configuration.GetSection(TwilioSmsOptions.ConfigSection));
        services.AddTransient<ISmsSender>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TwilioSmsOptions>>();
            var inner = new TwilioSmsSender(options, sp.GetRequiredService<ILogger<TwilioSmsSender>>());
            return new OverrideRecipientSmsSender(inner, options.Value.OverrideRecipient);
        });
        return services;
    }
}

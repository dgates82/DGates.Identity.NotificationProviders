using DGates.Identity.NotificationProviders.ExtensionMethods;
using DGates.Identity.NotificationProviders.Options.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostmarkDotNet;

namespace DGates.Identity.NotificationProviders.Providers.Email;

/// <summary>
/// Alternate <see cref="IEmailSender"/> implementation using Postmark. Not
/// wired up by default (see the commented-out <c>AddPostMarkEmailSender</c>
/// call in <c>Program.cs</c>); swap in for <see cref="SmtpEmailSender"/> when
/// a real transactional-email provider is needed.
/// </summary>
public class PostMarkEmailSender : IEmailSender
{
    private readonly ILogger _logger;

    readonly IOptions<PostMarkEmailOptions> _options;

    /// <summary>Creates the sender with its injected logger and Postmark configuration options.</summary>
    public PostMarkEmailSender(ILogger<PostMarkEmailSender> logger, IOptions<PostMarkEmailOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    /// <summary>Sends an HTML email via the Postmark API.</summary>
    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        _logger.LogDebug($"SendMailAsync | toEmail: {toEmail} | subject: {subject} | message: {message}");
        _logger.LogDebug($"options: {_options.ToJson()}");

        var client = new PostmarkClient(_options.Value.ApiKey);
        var from = _options.Value.FromAddress;

        var msg = new PostmarkMessage
        {
            From = from,
            To = toEmail,
            TrackOpens = true,
            Subject = subject,
            HtmlBody = message
        };

        var response = await client.SendMessageAsync(msg);

        if (response.Status == PostmarkStatus.Success)
        {
            _logger.LogInformation($"Message sent to {toEmail}");
        }
        else
        {
            _logger.LogError($"Error sending message to {toEmail}: {response.Message}");
            throw new InvalidOperationException($"Postmark send failed for {toEmail}: {response.Message}");
        }
    }
}

/// <summary>Registers <see cref="PostMarkEmailSender"/> as the <see cref="IEmailSender"/> implementation.</summary>
public static class PostMarkEmailSenderServiceCollectionExtensions
{
    /// <summary>Binds <see cref="PostMarkEmailOptions"/> from configuration and registers <see cref="PostMarkEmailSender"/>, wrapped with <see cref="OverrideRecipientEmailSender"/>.</summary>
    public static IServiceCollection AddPostMarkEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PostMarkEmailOptions>(configuration.GetSection(PostMarkEmailOptions.ConfigSection));
        services.AddTransient<IEmailSender>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PostMarkEmailOptions>>();
            var inner = new PostMarkEmailSender(sp.GetRequiredService<ILogger<PostMarkEmailSender>>(), options);
            return new OverrideRecipientEmailSender(inner, options.Value.OverrideRecipient);
        });
        return services;
    }
}

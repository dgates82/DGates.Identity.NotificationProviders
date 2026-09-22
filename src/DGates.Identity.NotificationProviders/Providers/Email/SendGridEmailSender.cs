using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DGates.Identity.NotificationProviders.ExtensionMethods;
using DGates.Identity.NotificationProviders.Options.Email;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DGates.Identity.NotificationProviders.Providers.Email
{
    /// <summary>
    /// <see cref="IEmailSender"/> implementation using SendGrid, for a real
    /// transactional-email provider instead of plain SMTP.
    /// </summary>
    public class SendGridEmailSender : IEmailSender
    {

        private readonly ILogger _logger;

        readonly IOptions<SendGridEmailOptions> _options;


        /// <summary>Creates the sender with its injected logger and SendGrid configuration options.</summary>
        public SendGridEmailSender(ILogger<SendGridEmailSender> logger, IOptions<SendGridEmailOptions> options)
        {
            _logger = logger;
            _options = options;

        }


        /// <summary>Sends an HTML email via the SendGrid API.</summary>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogDebug("SendMailAsync called");
            _logger.LogDebug("options: {Options}", _options.ToJson());

            var apiKey = _options.Value.ApiKey;
            var client = string.IsNullOrEmpty(_options.Value.BaseUrlOverride)
                ? new SendGridClient(apiKey)
                : new SendGridClient(apiKey, host: _options.Value.BaseUrlOverride);
            var from = new EmailAddress(_options.Value.FromAddress, _options.Value.FromName);

            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Message sent to {Email}", email.MaskEmail());
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Error sending message to {Email}: {StatusCode} {Body}", email.MaskEmail(), response.StatusCode, body);
                throw new InvalidOperationException($"SendGrid send failed: {response.StatusCode} {body}");
            }

        }

    }

    /// <summary>Registers <see cref="SendGridEmailSender"/> as the <see cref="IEmailSender"/> implementation.</summary>
    public static class SendGridEmailSenderServiceCollectionExtensions
    {
        /// <summary>Binds <see cref="SendGridEmailOptions"/> from configuration and registers <see cref="SendGridEmailSender"/>, wrapped with <see cref="OverrideRecipientEmailSender"/>.</summary>
        public static IServiceCollection AddSendGridEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SendGridEmailOptions>(configuration.GetSection(SendGridEmailOptions.ConfigSection));
            services.AddTransient<IEmailSender>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<SendGridEmailOptions>>();
                var inner = new SendGridEmailSender(sp.GetRequiredService<ILogger<SendGridEmailSender>>(), options);
                return new OverrideRecipientEmailSender(inner, options.Value.OverrideRecipient);
            });
            return services;
        }
    }
}

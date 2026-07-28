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
    /// Alternate <see cref="IEmailSender"/> implementation using SendGrid. Not
    /// wired up by default (see the commented-out <c>AddSendGridEmailSender</c>
    /// call in <c>Program.cs</c>); swap in for <see cref="SmtpEmailSender"/>
    /// when a real transactional-email provider is needed.
    /// </summary>
    public class SendGridEmailSender : IEmailSender
    {

        private readonly ILogger _logger;

        private bool IsOverrideRecipient => !string.IsNullOrEmpty(_options.Value.OverrideRecipient);
        private string OverrideRecipient => _options.Value.OverrideRecipient;

        readonly IOptions<SendGridEmailOptions> _options;


        /// <summary>Creates the sender with its injected logger and SendGrid configuration options.</summary>
        public SendGridEmailSender(ILogger<SmtpEmailSender> logger, IOptions<SendGridEmailOptions> options)
        {
            _logger = logger;
            _options = options;

        }


        /// <summary>
        /// Sends an HTML email via the SendGrid API. If
        /// <see cref="SendGridEmailOptions.OverrideRecipient"/> is configured, the
        /// email is redirected there instead, with the original recipient appended
        /// to the subject line.
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            _logger.LogDebug($"SendMailAsync | toEmail: {toEmail} | subject: {subject} | message: {message}");
            _logger.LogDebug($"options: {_options.ToJson()}");

            var apiKey = _options.Value.ApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_options.Value.FromAddress, _options.Value.FromName);

            var testSubject = IsOverrideRecipient ? $" - Original Recipient: {toEmail}" : "";
            var finalSubject = $"{subject}{testSubject}";

            var finalRecipient = IsOverrideRecipient ? OverrideRecipient : toEmail;

            var to = new EmailAddress(finalRecipient);
            var msg = MailHelper.CreateSingleEmail(from, to, finalSubject, "", message);
            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Message sent to {finalRecipient}");
            }
            else
            {
                _logger.LogError($"Error sending message to {finalRecipient}");
            }                        

        }

    }

    /// <summary>Registers <see cref="SendGridEmailSender"/> as the <see cref="IEmailSender"/> implementation.</summary>
    public static class SendGridEmailSenderServiceCollectionExtensions
    {
        /// <summary>Binds <see cref="SendGridEmailOptions"/> from configuration and registers <see cref="SendGridEmailSender"/>.</summary>
        public static IServiceCollection AddSendGridEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SendGridEmailOptions>(configuration.GetSection(SendGridEmailOptions.ConfigSection));
            services.AddTransient<IEmailSender, SendGridEmailSender>();
            return services;
        }
    }
}

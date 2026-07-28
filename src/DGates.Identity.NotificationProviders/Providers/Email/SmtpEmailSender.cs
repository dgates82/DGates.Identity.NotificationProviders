using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DGates.Identity.NotificationProviders.ExtensionMethods;
using DGates.Identity.NotificationProviders.Options.Email;
using System.Net;
using System.Net.Mail;

namespace DGates.Identity.NotificationProviders.Providers.Email
{
    /// <summary>
    /// Default <see cref="IEmailSender"/> implementation, sending mail over SMTP.
    /// Configured out of the box to point at the Mailpit container from
    /// <c>docker-compose.yml</c>, so a fresh clone has a working email path with
    /// no external account or API key.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly ILogger _logger;

        private bool IsOverrideRecipient => !string.IsNullOrEmpty(_options.Value.OverrideRecipient);
        private string OverrideRecipient => _options.Value.OverrideRecipient;

        readonly IOptions<SmtpEmailOptions> _options;


        /// <summary>Creates the sender with its injected logger and SMTP configuration options.</summary>
        public SmtpEmailSender(ILogger<SmtpEmailSender> logger, IOptions<SmtpEmailOptions> options)
        {
            _logger = logger;
            _options = options;

        }


        /// <summary>
        /// Sends an HTML email via SMTP. If <see cref="SmtpEmailOptions.OverrideRecipient"/>
        /// is configured, the email is redirected there instead, with the
        /// original recipient appended to the subject line — useful for testing
        /// against a real inbox without emailing real users.
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            _logger.LogDebug($"SendMailAsync | toEmail: {toEmail} | subject: {subject} | message: {message}");
            _logger.LogDebug($"options: {_options.ToJson()}");

            var msg = new MailMessage();
            msg.From = new MailAddress(_options.Value.FromAddress);

            var testSubject = IsOverrideRecipient ? $" - Original Recipient: {toEmail}" : "";
            var finalSubject = $"{subject}{testSubject}";
            msg.Subject = finalSubject;
            msg.Body = message;

            var finalRecipient = IsOverrideRecipient ? OverrideRecipient : toEmail;

            msg.To.Add(finalRecipient);

            msg.IsBodyHtml = true;

            using var smtpClient = new SmtpClient
            {
                UseDefaultCredentials = string.IsNullOrEmpty(_options.Value.UserName),
                EnableSsl = _options.Value.EnableSsl,
                Host = _options.Value.Host,
                Port = int.Parse(_options.Value.Port)                
            };

            if (!string.IsNullOrEmpty(_options.Value.UserName)) 
            {
                smtpClient.Credentials = new NetworkCredential(_options.Value.UserName, _options.Value.Password);
            }

            await smtpClient.SendMailAsync(msg);

            _logger.LogInformation($"Message sent to {finalRecipient}");

        }

    }

    /// <summary>Registers <see cref="SmtpEmailSender"/> as the <see cref="IEmailSender"/> implementation.</summary>
    public static class SmtpEmailSenderServiceCollectionExtensions
    {
        /// <summary>Binds <see cref="SmtpEmailOptions"/> from configuration and registers <see cref="SmtpEmailSender"/>.</summary>
        public static IServiceCollection AddSmtpEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpEmailOptions>(configuration.GetSection(SmtpEmailOptions.ConfigSection));
            services.AddTransient<IEmailSender, SmtpEmailSender>();
            return services;
        }
    }
}

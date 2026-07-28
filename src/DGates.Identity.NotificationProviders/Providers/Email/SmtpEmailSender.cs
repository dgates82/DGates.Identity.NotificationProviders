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

        readonly IOptions<SmtpEmailOptions> _options;

        /// <summary>Creates the sender with its injected logger and SMTP configuration options.</summary>
        public SmtpEmailSender(ILogger<SmtpEmailSender> logger, IOptions<SmtpEmailOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        /// <summary>Sends an HTML email via SMTP.</summary>
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            _logger.LogDebug($"SendMailAsync | toEmail: {toEmail} | subject: {subject} | message: {message}");
            _logger.LogDebug($"options: {_options.ToJson()}");

            var msg = new MailMessage();
            msg.From = new MailAddress(_options.Value.FromAddress);
            msg.Subject = subject;
            msg.Body = message;
            msg.To.Add(toEmail);
            msg.IsBodyHtml = true;

            using var smtpClient = new SmtpClient
            {
                UseDefaultCredentials = string.IsNullOrEmpty(_options.Value.UserName),
                EnableSsl = _options.Value.EnableSsl,
                Host = _options.Value.Host,
                Port = _options.Value.Port
            };

            if (!string.IsNullOrEmpty(_options.Value.UserName))
            {
                smtpClient.Credentials = new NetworkCredential(_options.Value.UserName, _options.Value.Password);
            }

            await smtpClient.SendMailAsync(msg);

            _logger.LogInformation($"Message sent to {toEmail}");

        }

    }

    /// <summary>Registers <see cref="SmtpEmailSender"/> as the <see cref="IEmailSender"/> implementation.</summary>
    public static class SmtpEmailSenderServiceCollectionExtensions
    {
        /// <summary>Binds <see cref="SmtpEmailOptions"/> from configuration and registers <see cref="SmtpEmailSender"/>, wrapped with <see cref="OverrideRecipientEmailSender"/>.</summary>
        public static IServiceCollection AddSmtpEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpEmailOptions>(configuration.GetSection(SmtpEmailOptions.ConfigSection));
            services.AddTransient<IEmailSender>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<SmtpEmailOptions>>();
                var inner = new SmtpEmailSender(sp.GetRequiredService<ILogger<SmtpEmailSender>>(), options);
                return new OverrideRecipientEmailSender(inner, options.Value.OverrideRecipient);
            });
            return services;
        }
    }
}

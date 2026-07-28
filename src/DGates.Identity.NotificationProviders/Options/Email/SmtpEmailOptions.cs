namespace DGates.Identity.NotificationProviders.Options.Email
{
    /// <summary>Configuration for <see cref="Providers.Email.SmtpEmailSender"/>, bound from the <see cref="ConfigSection"/> config section.</summary>
    public class SmtpEmailOptions
    {
        /// <summary>The configuration section name this options class binds to.</summary>
        public const string ConfigSection = "SmtpEmailConfigs";

        /// <summary>The SMTP server host.</summary>
        public required string Host { get; set; }

        /// <summary>The SMTP server port.</summary>
        public required string Port { get; set; }

        /// <summary>The "From" address for outgoing emails.</summary>
        public required string FromAddress { get; set; }

        /// <summary>When set, all emails are redirected here instead of their real recipient, for testing against a real inbox without emailing real users.</summary>
        public string OverrideRecipient { get; set; } = "";

        /// <summary>The SMTP auth username. When empty, the client falls back to default (unauthenticated) credentials — the local Mailpit setup doesn't need one.</summary>
        public string UserName { get; set; } = "";

        /// <summary>The SMTP auth password, used only when <see cref="UserName"/> is set.</summary>
        public string Password { get; set; } = "";

        /// <summary>Whether to use SSL/TLS for the SMTP connection.</summary>
        public bool EnableSsl { get; set; } = true;
    }
}

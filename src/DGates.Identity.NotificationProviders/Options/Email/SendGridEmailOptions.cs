namespace DGates.Identity.NotificationProviders.Options.Email
{
    /// <summary>Configuration for <see cref="Providers.Email.SendGridEmailSender"/>, bound from the <see cref="ConfigSection"/> config section.</summary>
    public class SendGridEmailOptions
    {
        /// <summary>The configuration section name this options class binds to.</summary>
        public const string ConfigSection = "SendGridEmailConfigs";

        /// <summary>The SendGrid API key.</summary>
        public required string ApiKey { get; set; }

        /// <summary>The "From" address for outgoing emails.</summary>
        public required string FromAddress { get; set; }

        /// <summary>The "From" display name for outgoing emails.</summary>
        public required string FromName { get; set; }

        /// <summary>When set, all emails are redirected here instead of their real recipient, for testing against a real inbox without emailing real users.</summary>
        public string OverrideRecipient { get; set; } = "";

    }
}

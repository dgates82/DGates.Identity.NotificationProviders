using Newtonsoft.Json;

namespace DGates.Identity.NotificationProviders.Options.Email
{
    /// <summary>Configuration for <see cref="Providers.Email.SendGridEmailSender"/>, bound from the <see cref="ConfigSection"/> config section.</summary>
    public class SendGridEmailOptions
    {
        /// <summary>The configuration section name this options class binds to.</summary>
        public const string ConfigSection = "SendGridEmailConfigs";

        /// <summary>The SendGrid API key.</summary>
        [JsonIgnore]
        public required string ApiKey { get; set; }

        /// <summary>The "From" address for outgoing emails.</summary>
        public required string FromAddress { get; set; }

        /// <summary>The "From" display name for outgoing emails.</summary>
        public required string FromName { get; set; }

        /// <summary>When set, all emails are redirected here instead of their real recipient, for testing against a real inbox without emailing real users.</summary>
        public string OverrideRecipient { get; set; } = "";

        /// <summary>
        /// When set, redirects all SendGrid API requests to this URL instead of
        /// api.sendgrid.com - for local dev, point this at a SendGrid-compatible
        /// mock (e.g. this repo's docker/sendgrid-mock) instead of a real SendGrid account.
        /// </summary>
        public string BaseUrlOverride { get; set; } = "";

    }
}

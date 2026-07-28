using Newtonsoft.Json;

namespace DGates.Identity.NotificationProviders.Options.Email;

/// <summary>Configuration for <see cref="Providers.Email.PostMarkEmailSender"/>, bound from the <see cref="ConfigSection"/> config section.</summary>
public class PostMarkEmailOptions
{
    /// <summary>The configuration section name this options class binds to.</summary>
    public const string ConfigSection = "PostMarkEmailConfigs";

    /// <summary>The Postmark server API key.</summary>
    [JsonIgnore]
    public required string ApiKey { get; set; }

    /// <summary>The "From" address for outgoing emails.</summary>
    public required string FromAddress { get; set; }

    /// <summary>When set, all emails are redirected here instead of their real recipient, for testing against a real inbox without emailing real users.</summary>
    public string OverrideRecipient { get; set; } = "";

    /// <summary>
    /// When set, redirects all Postmark API requests to this URL instead of
    /// api.postmarkapp.com - for local dev, point this at a Postmark-compatible
    /// mock (e.g. this repo's docker/postmark-mock) instead of a real Postmark account.
    /// </summary>
    public string BaseUrlOverride { get; set; } = "";
}
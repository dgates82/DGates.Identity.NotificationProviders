using Newtonsoft.Json;

namespace DGates.Identity.NotificationProviders.Options.Sms;

/// <summary>Configuration for <see cref="Providers.Sms.SnsSmsSender"/>, bound from the <see cref="ConfigSection"/> config section.</summary>
public class SnsSmsOptions
{
    /// <summary>The configuration section name this options class binds to.</summary>
    public const string ConfigSection = "SnsSmsConfigs";

    /// <summary>The AWS region to publish through, e.g. "us-east-1".</summary>
    public required string Region { get; set; }

    /// <summary>
    /// Static AWS access key, for environments (e.g. LocalStack) that need explicit
    /// credentials instead of the default AWS credential chain. Leave empty in production
    /// to use the credential chain (IAM role, environment, etc.) rather than storing keys here.
    /// </summary>
    public string AccessKey { get; set; } = "";

    /// <summary>Static AWS secret key, paired with <see cref="AccessKey"/>. Ignored when <see cref="AccessKey"/> is empty.</summary>
    [JsonIgnore]
    public string SecretKey { get; set; } = "";

    /// <summary>
    /// When set, redirects all SNS API requests to this URL instead of the real AWS
    /// endpoint - for local dev, point this at LocalStack instead of a real AWS account.
    /// </summary>
    public string ServiceUrlOverride { get; set; } = "";

    /// <summary>When set, all SMS messages are redirected here instead of their real recipient, for testing against a real phone without texting real users.</summary>
    public string OverrideRecipient { get; set; } = "";

    /// <summary>The alphanumeric sender ID shown to recipients where supported (AWS.SNS.SMS.SenderID). Leave empty to omit.</summary>
    public string SenderId { get; set; } = "";

    /// <summary>The SNS SMS message type - "Transactional" (default, prioritizes delivery, e.g. for 2FA codes) or "Promotional" (AWS.SNS.SMS.SMSType).</summary>
    public string SmsType { get; set; } = "Transactional";
}

namespace DGates.Identity.NotificationProviders.Options.Sms;

/// <summary>Configuration for <see cref="Providers.Sms.TwilioSmsSender"/>, bound from the <see cref="ConfigSection"/> config section.</summary>
public class TwilioSmsOptions
{
    /// <summary>The configuration section name this options class binds to.</summary>
    public const string ConfigSection = "TwilioSmsConfigs";

    /// <summary>The Twilio account SID.</summary>
    public required string AccountSid { get; set; }

    /// <summary>The Twilio auth token.</summary>
    public required string AuthToken { get; set; }

    /// <summary>The Twilio phone number to send SMS from.</summary>
    public required string FromNumber { get; set; }

    /// <summary>When set, all SMS messages are redirected here instead of their real recipient, for testing against a real phone without texting real users.</summary>
    public string OverrideRecipient { get; set; } = "";

    /// <summary>
    /// When set, redirects all Twilio API requests to this URL instead of
    /// api.twilio.com - for local dev, point this at a Twilio-compatible
    /// mock (e.g. twillio-sms-mock) instead of a real Twilio account.
    /// </summary>
    public string BaseUrlOverride { get; set; } = "";
}
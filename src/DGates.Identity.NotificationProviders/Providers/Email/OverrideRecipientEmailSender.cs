using Microsoft.AspNetCore.Identity.UI.Services;

namespace DGates.Identity.NotificationProviders.Providers.Email;

/// <summary>
/// Decorates an <see cref="IEmailSender"/>. When <see cref="OverrideRecipient"/> is set, every
/// email is redirected there instead of its real recipient, with the original recipient
/// appended to the subject line - for testing against a real inbox without emailing real users.
/// </summary>
public class OverrideRecipientEmailSender : IEmailSender
{
    private readonly IEmailSender _inner;
    private readonly string _overrideRecipient;

    /// <summary>Wraps <paramref name="inner"/>, redirecting to <paramref name="overrideRecipient"/> when non-empty.</summary>
    public OverrideRecipientEmailSender(IEmailSender inner, string overrideRecipient)
    {
        _inner = inner;
        _overrideRecipient = overrideRecipient;
    }

    /// <inheritdoc />
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(_overrideRecipient))
        {
            return _inner.SendEmailAsync(email, subject, htmlMessage);
        }

        var finalSubject = $"{subject} - Original Recipient: {email}";
        return _inner.SendEmailAsync(_overrideRecipient, finalSubject, htmlMessage);
    }
}

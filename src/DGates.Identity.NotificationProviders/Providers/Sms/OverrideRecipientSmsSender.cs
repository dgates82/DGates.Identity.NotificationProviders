using DGates.Identity.NotificationProviders.Abstractions;

namespace DGates.Identity.NotificationProviders.Providers.Sms;

/// <summary>
/// Decorates an <see cref="ISmsSender"/>. When <see cref="OverrideRecipient"/> is set, every
/// message is redirected there instead of its real recipient, with the original number
/// appended to the message body - for testing against a real phone without texting real users.
/// </summary>
public class OverrideRecipientSmsSender : ISmsSender
{
    private readonly ISmsSender _inner;
    private readonly string _overrideRecipient;

    /// <summary>Wraps <paramref name="inner"/>, redirecting to <paramref name="overrideRecipient"/> when non-empty.</summary>
    public OverrideRecipientSmsSender(ISmsSender inner, string overrideRecipient)
    {
        _inner = inner;
        _overrideRecipient = overrideRecipient;
    }

    /// <inheritdoc />
    public Task<string> SendSmsAsync(string number, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_overrideRecipient))
        {
            return _inner.SendSmsAsync(number, message, cancellationToken);
        }

        var finalMessage = $"{message} - Original Recipient: {number}";
        return _inner.SendSmsAsync(_overrideRecipient, finalMessage, cancellationToken);
    }
}

namespace DGates.Identity.NotificationProviders.Abstractions;

/// <summary>Sends SMS messages, e.g. one-time 2FA codes, to a phone number.</summary>
public interface ISmsSender
{
    /// <summary>
    /// Sends <paramref name="message"/> to <paramref name="number"/>, returning the provider's
    /// message ID. Implementations should throw on delivery failure rather than fail silently,
    /// so callers see consistent behavior regardless of which registered <see cref="ISmsSender"/>
    /// they're using.
    /// </summary>
    public Task<string> SendSmsAsync(string number, string message, CancellationToken cancellationToken = default);
}
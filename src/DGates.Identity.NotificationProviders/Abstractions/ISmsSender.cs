namespace DGates.Identity.NotificationProviders.Abstractions;

/// <summary>Sends SMS messages, e.g. one-time 2FA codes, to a phone number.</summary>
public interface ISmsSender
{
    /// <summary>Sends <paramref name="message"/> to <paramref name="number"/>.</summary>
    public Task SendSmsAsync(string number, string message);
}
namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Notifications;

/// <summary>
/// Consumes an <see cref="INotificationSender"/>, which can only be supplied by a blueprint.
/// </summary>
/// <param name="sender">The sender used to deliver notifications.</param>
public sealed class NotificationService(INotificationSender sender)
{
    /// <summary>
    /// Gets the name of the channel notifications are sent on.
    /// </summary>
    /// <returns>The channel name.</returns>
    public string GetChannel()
    {
        return sender.GetChannel();
    }

    /// <summary>
    /// Sends the given message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>True if the message was sent.</returns>
    public Task<bool> NotifyAsync(string message)
    {
        return sender.SendAsync(message, CancellationToken.None);
    }
}

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Notifications;

/// <summary>
/// Sends notifications, with no implementation of its own so it can only be supplied by a blueprint.
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// Gets the name of the channel notifications are sent on.
    /// </summary>
    /// <returns>The channel name.</returns>
    string GetChannel();

    /// <summary>
    /// Sends the given message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the message was sent.</returns>
    Task<bool> SendAsync(string message, CancellationToken cancellationToken);
}

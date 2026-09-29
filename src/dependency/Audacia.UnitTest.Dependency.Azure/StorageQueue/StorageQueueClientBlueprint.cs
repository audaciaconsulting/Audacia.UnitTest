using Audacia.UnitTest.Dependency.Customisations;
using Azure.Storage.Queues;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.StorageQueue;

/// <summary>
/// A blueprint for a <see cref="QueueClient"/>.
/// This is the object for sending messages to a storage queue.
/// </summary>
public sealed class StorageQueueClientBlueprint : CustomisedBlueprintDependency<QueueClient>
{
    /// <summary>
    /// Creates a default <see cref="StorageQueueClientBlueprint"/> where any message sent will be accepted.
    /// </summary>
    public StorageQueueClientBlueprint()
    {
        var defaultBehavior = new BlueprintCustomisation<QueueClient, Task>(
            sender =>
                sender.SendMessageAsync(
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>()),
            Task.CompletedTask);
        Customisations.Add(defaultBehavior);
    }

    /// <summary>
    /// Creates a <see cref="StorageQueueClientBlueprint"/> where any message sent will throw an exception.
    /// </summary>
    /// <returns>Blueprint configured to throw an exception when sending a message.</returns>
    public static StorageQueueClientBlueprint ThrowExceptionWhenSending()
    {
        var blueprint = new StorageQueueClientBlueprint();
        var exceptionThrowingCustomisation = new BlueprintCustomisation<QueueClient, Task>(
            sender =>
                sender.SendMessageAsync(
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>()),
            Task.FromException(new InvalidOperationException()));
        blueprint.Customisations.Add(exceptionThrowingCustomisation);

        return blueprint;
    }
}
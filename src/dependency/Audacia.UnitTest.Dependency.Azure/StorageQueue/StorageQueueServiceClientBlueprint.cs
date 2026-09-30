using Audacia.UnitTest.Dependency.Customisations;
using Azure.Storage.Queues;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.StorageQueue;

/// <summary>
/// A blueprint for a <see cref="QueueServiceClient"/>.
/// This is the object for managing all interactions with queues in a storage account.
/// </summary>
public sealed class StorageQueueServiceClientBlueprint : CustomisedBlueprintDependency<QueueServiceClient>
{
    /// <summary>
    /// Creates a default <see cref="StorageQueueServiceClientBlueprint"/> where any queue name
    /// will resolve to a queue client where any message sent will be accepted.
    /// </summary>
    public StorageQueueServiceClientBlueprint()
    {
        var defaultStorageQueueClient = new StorageQueueClientBlueprint().Build();

        var defaultStorageQueueClientCustomisation = new BlueprintCustomisation<QueueServiceClient, QueueClient>(
            serviceClient => serviceClient.GetQueueClient(Arg.Any<string>()),
            defaultStorageQueueClient);

        Customisations.Add(defaultStorageQueueClientCustomisation);
    }

    /// <summary>
    /// Creates a <see cref="StorageQueueServiceClientBlueprint"/> where the
    /// given <paramref name="queueName"/> will resolve to the given <paramref name="queueClient"/>.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="queueClient">Instance of <see cref="QueueClient"/>.</param>
    /// <returns>Blueprint configured with the <paramref name="queueClient"/> for <paramref name="queueName"/>.</returns>
    public static StorageQueueServiceClientBlueprint WithNamedQueue(
        string queueName,
        QueueClient queueClient)
    {
        var blueprint = new StorageQueueServiceClientBlueprint();
        var defaultStorageQueueClientCustomisation = new BlueprintCustomisation<QueueServiceClient, QueueClient>(
            storageQueueClient => storageQueueClient.GetQueueClient(Arg.Is<string>(match => match == queueName)),
            queueClient);

        blueprint.Customisations.Add(defaultStorageQueueClientCustomisation);

        return blueprint;
    }
}
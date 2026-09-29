using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;

namespace Audacia.UnitTest.Dependency.Azure.StorageQueue;

/// <summary>
/// A blueprint for an <see cref="IAzureClientFactory{TClient}"/> for a <see cref="QueueServiceClient"/>.
/// </summary>
public sealed class AzureClientFactoryStorageQueueBlueprint : AzureClientFactoryBlueprint<QueueServiceClient>
{
    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="QueueServiceClient"/> with a default client and queue client.
    /// This will handle any named client and any queue name, successfully sending a message.
    /// </summary>
    public AzureClientFactoryStorageQueueBlueprint()
    {
        var client = new StorageQueueServiceClientBlueprint().Build();
        SetupAnyNamedClient(client);
    }

    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="QueueServiceClient"/> with the given <paramref name="clientName"/> and <paramref name="client"/> instance.
    /// </summary>
    /// <param name="clientName">The name of the client.</param>
    /// <param name="client">The instance of the client.</param>
    public AzureClientFactoryStorageQueueBlueprint(string clientName, QueueServiceClient client)
    {
        SetupNamedClient(clientName, client);
    }

    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="QueueServiceClient"/> with the given <paramref name="clientName"/> for the <paramref name="queueName"/>.
    /// This queue client will accept any message sent.
    /// </summary>
    /// <param name="clientName">The name of the client.</param>
    /// <param name="queueName">The name of the queue.</param>
    /// <returns>Blueprint for creating a service bus client.</returns>
    public static AzureClientFactoryStorageQueueBlueprint Create(
        string clientName,
        string queueName)
    {
        var queueClient = new StorageQueueClientBlueprint().Build();
        var client = StorageQueueServiceClientBlueprint
            .WithNamedQueue(queueName, queueClient)
            .Build();

        return new AzureClientFactoryStorageQueueBlueprint(clientName, client);
    }

    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="QueueServiceClient"/> with the given <paramref name="clientName"/> for the <paramref name="queueName"/>.
    /// This queue client will throw an exception for any message sent.
    /// </summary>
    /// <param name="clientName">The name of the client.</param>
    /// <param name="queueName">The name of the queue/topic.</param>
    /// <returns>Blueprint for creating a service bus client.</returns>
    public static AzureClientFactoryStorageQueueBlueprint CreateWithFailure(
        string clientName,
        string queueName)
    {
        var sender = StorageQueueClientBlueprint.ThrowExceptionWhenSending()
            .Build();
        var client = StorageQueueServiceClientBlueprint.WithNamedQueue(queueName, sender)
            .Build();

        return new AzureClientFactoryStorageQueueBlueprint(clientName, client);
    }
}
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;

namespace Audacia.UnitTest.Dependency.Azure.ServiceBus;

/// <summary>
/// Blueprint for a dependency of <see cref="IAzureClientFactory{TClient}"/> for a <see cref="ServiceBusClient"/>.
/// </summary>
public sealed class AzureClientFactoryServiceBusBlueprint : AzureClientFactoryBlueprint<ServiceBusClient>
{
    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="ServiceBusClient"/> with a default client and sender.
    /// This will handle any named client and any sender name, successfully sending a message.
    /// </summary>
    public AzureClientFactoryServiceBusBlueprint()
    {
        var client = new ServiceBusClientBlueprint().Build();
        SetupAnyNamedClient(client);
    }

    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="ServiceBusClient"/> with the given <paramref name="clientName"/> and <paramref name="client"/> instance.
    /// </summary>
    /// <param name="clientName">The name of the client.</param>
    /// <param name="client">The instance of the client.</param>
    public AzureClientFactoryServiceBusBlueprint(string clientName, ServiceBusClient client)
    {
        SetupNamedClient(clientName, client);
    }

    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="ServiceBusClient"/> with the given <paramref name="clientNames"/> and <paramref name="client"/> instance.
    /// </summary>
    /// <param name="clientNames">The names of the clients.</param>
    /// <param name="client">The instance of the client.</param>
    public AzureClientFactoryServiceBusBlueprint(string[] clientNames, ServiceBusClient client)
    {
        SetupNamedClients(clientNames, client);
    }

    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="ServiceBusClient"/> with the given <paramref name="clientName"/> for the <paramref name="entityName"/>.
    /// This sender will accept any message sent.
    /// </summary>
    /// <param name="clientName">The name of the client.</param>
    /// <param name="entityName">The name of the queue/topic.</param>
    /// <returns>Blueprint for creating a service bus client.</returns>
    public static AzureClientFactoryServiceBusBlueprint Create(
        string clientName,
        string entityName)
    {
        var sender = new ServiceBusSenderBlueprint().Build();
        var client = ServiceBusClientBlueprint
            .WithNamedSender(entityName, sender)
            .Build();

        return new AzureClientFactoryServiceBusBlueprint(clientName, client);
    }

    /// <summary>
    /// Creates a blueprint for an <see cref="IAzureClientFactory{TClient}"/>
    /// for a <see cref="ServiceBusClient"/> with the given <paramref name="clientName"/> for the <paramref name="entityName"/>.
    /// This sender will throw an exception for any message sent.
    /// </summary>
    /// <param name="clientName">The name of the client.</param>
    /// <param name="entityName">The name of the queue/topic.</param>
    /// <returns>Blueprint for creating a service bus client.</returns>
    public static AzureClientFactoryServiceBusBlueprint CreateWithFailure(
        string clientName,
        string entityName)
    {
        var sender = ServiceBusSenderBlueprint.ThrowExceptionWhenSending()
            .Build();
        var client = ServiceBusClientBlueprint.WithNamedSender(entityName, sender)
            .Build();

        return new AzureClientFactoryServiceBusBlueprint(clientName, client);
    }
}
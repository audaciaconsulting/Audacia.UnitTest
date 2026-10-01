using Audacia.UnitTest.Dependency.Blueprints;
using Azure.Messaging.ServiceBus;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.ServiceBus;

/// <summary>
/// A blueprint for a <see cref="ServiceBusClient"/>.
/// This is the object for managing all interactions with entities in a service bus namespace.
/// </summary>
public sealed class ServiceBusClientBlueprint : BlueprintDependency<ServiceBusClient>
{
    /// <summary>
    /// Creates a default <see cref="ServiceBusClientBlueprint"/> where any sender name
    /// will resolve to a sender where any message sent will be accepted.
    /// </summary>
    public ServiceBusClientBlueprint()
    {
        var defaultServiceBusSender = new ServiceBusSenderBlueprint().Build();

        var defaultServiceBusClientCustomisation = new BlueprintCustomisation<ServiceBusClient, ServiceBusSender>(
            sender => sender.CreateSender(Arg.Any<string>()),
            defaultServiceBusSender);

        Customisations.Add(defaultServiceBusClientCustomisation);
    }

    /// <summary>
    /// Creates a <see cref="ServiceBusClientBlueprint"/> where the
    /// given <paramref name="entityName"/> will resolve to the given <paramref name="sender"/>.
    /// </summary>
    /// <param name="entityName">The name of the queue or topic.</param>
    /// <param name="sender">Instance of <see cref="ServiceBusSender"/>.</param>
    /// <returns>Blueprint configured with the <paramref name="sender"/> for <paramref name="entityName"/>.</returns>
    public static ServiceBusClientBlueprint WithNamedSender(
        string entityName,
        ServiceBusSender sender)
    {
        var blueprint = new ServiceBusClientBlueprint();
        var defaultServiceBusClientCustomisation = new BlueprintCustomisation<ServiceBusClient, ServiceBusSender>(
            serviceBusClient => serviceBusClient.CreateSender(Arg.Is<string>(match => match == entityName)),
            sender);

        blueprint.Customisations.Add(defaultServiceBusClientCustomisation);

        return blueprint;
    }
}
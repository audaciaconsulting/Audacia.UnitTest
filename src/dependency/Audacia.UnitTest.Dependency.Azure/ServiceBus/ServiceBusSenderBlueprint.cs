using Audacia.UnitTest.Dependency.Customisations;
using Azure.Messaging.ServiceBus;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.ServiceBus;

/// <summary>
/// A blueprint for a <see cref="ServiceBusSender"/>.
/// This is the object for sending messages to a service bus queue/topic.
/// </summary>
public sealed class ServiceBusSenderBlueprint : CustomisedBlueprintDependency<ServiceBusSender>
{
    /// <summary>
    /// Creates a default <see cref="ServiceBusSenderBlueprint"/> where any message sent will be accepted.
    /// </summary>
    public ServiceBusSenderBlueprint()
    {
        var defaultBehavior = new BlueprintCustomisation<ServiceBusSender, Task>(
            sender =>
                sender.SendMessageAsync(
                    Arg.Any<ServiceBusMessage>(),
                    Arg.Any<CancellationToken>()),
            Task.CompletedTask);

        Customisations.Add(defaultBehavior);
    }

    /// <summary>
    /// Creates a <see cref="ServiceBusSenderBlueprint"/> where any message sent will throw an exception.
    /// </summary>
    /// <returns>Blueprint configured to throw an exception when sending a message.</returns>
    public static ServiceBusSenderBlueprint ThrowExceptionWhenSending()
    {
        var blueprint = new ServiceBusSenderBlueprint();

        var exceptionThrowingCustomisation = new BlueprintCustomisation<ServiceBusSender, Task>(
            sender =>
                sender.SendMessageAsync(
                    Arg.Any<ServiceBusMessage>(),
                    Arg.Any<CancellationToken>()),
            Task.FromException(new ServiceBusException()));

        blueprint.Customisations.Add(exceptionThrowingCustomisation);

        return blueprint;
    }
}
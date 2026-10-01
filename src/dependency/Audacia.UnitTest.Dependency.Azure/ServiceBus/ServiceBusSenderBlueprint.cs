using Audacia.UnitTest.Dependency.Blueprints;
using Azure.Messaging.ServiceBus;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.ServiceBus;

/// <summary>
/// A blueprint for a <see cref="ServiceBusSender"/>.
/// This is the object for sending messages to a service bus queue/topic.
/// Sending a single message, sending multiple messages, scheduling messages and creating a message batch are all covered.
/// </summary>
public sealed class ServiceBusSenderBlueprint : BlueprintDependency<ServiceBusSender>
{
    private const long ScheduledSequenceNumber = 1;

    /// <summary>
    /// Creates a default <see cref="ServiceBusSenderBlueprint"/> where any message sent or scheduled will be accepted.
    /// </summary>
    public ServiceBusSenderBlueprint()
    {
        Customisations.Add(new BlueprintCustomisation<ServiceBusSender, Task>(
            sender => sender.SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>()),
            Task.CompletedTask));

        Customisations.Add(new BlueprintCustomisation<ServiceBusSender, Task>(
            sender => sender.SendMessagesAsync(
                Arg.Any<IEnumerable<ServiceBusMessage>>(),
                Arg.Any<CancellationToken>()),
            Task.CompletedTask));

        Customisations.Add(new BlueprintCustomisation<ServiceBusSender, Task>(
            sender => sender.SendMessagesAsync(Arg.Any<ServiceBusMessageBatch>(), Arg.Any<CancellationToken>()),
            Task.CompletedTask));

        Customisations.Add(new BlueprintCustomisation<ServiceBusSender, long>(
            sender => sender.ScheduleMessageAsync(
                Arg.Any<ServiceBusMessage>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>()),
            ScheduledSequenceNumber));

        Customisations.Add(new BlueprintCustomisation<ServiceBusSender, IReadOnlyList<long>>(
            sender => sender.ScheduleMessagesAsync(
                Arg.Any<IEnumerable<ServiceBusMessage>>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>()),
            [ScheduledSequenceNumber]));

        Customisations.Add(new CreateMessageBatchCustomisation());
    }

    /// <summary>
    /// Creates a <see cref="ServiceBusSenderBlueprint"/> where any message sent or scheduled will throw an exception.
    /// </summary>
    /// <remarks><para>Creating a message batch still succeeds, so the failure is seen when the batch is sent.</para></remarks>
    /// <returns>Blueprint configured to throw an exception when sending or scheduling a message.</returns>
    public static ServiceBusSenderBlueprint ThrowExceptionWhenSending()
    {
        var blueprint = new ServiceBusSenderBlueprint();

        blueprint.Customisations.Add(new BlueprintCustomisation<ServiceBusSender, Task>(
            sender => sender.SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>()),
            Task.FromException(new ServiceBusException())));

        blueprint.Customisations.Add(new BlueprintCustomisation<ServiceBusSender, Task>(
            sender => sender.SendMessagesAsync(
                Arg.Any<IEnumerable<ServiceBusMessage>>(),
                Arg.Any<CancellationToken>()),
            Task.FromException(new ServiceBusException())));

        blueprint.Customisations.Add(new BlueprintCustomisation<ServiceBusSender, Task>(
            sender => sender.SendMessagesAsync(Arg.Any<ServiceBusMessageBatch>(), Arg.Any<CancellationToken>()),
            Task.FromException(new ServiceBusException())));

        blueprint.Customisations.Add(new BlueprintCustomisation<ServiceBusSender, long>(
            sender => sender.ScheduleMessageAsync(
                Arg.Any<ServiceBusMessage>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>()),
            new ServiceBusException()));

        blueprint.Customisations.Add(new BlueprintCustomisation<ServiceBusSender, IReadOnlyList<long>>(
            sender => sender.ScheduleMessagesAsync(
                Arg.Any<IEnumerable<ServiceBusMessage>>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>()),
            new ServiceBusException()));

        return blueprint;
    }
}

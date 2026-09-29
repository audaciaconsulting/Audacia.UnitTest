using Audacia.UnitTest.Dependency.Customisations;
using Azure.Messaging.ServiceBus;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.ServiceBus;

/// <summary>
/// Customises a <see cref="ServiceBusSender"/> so that creating a message batch returns a usable
/// <see cref="ServiceBusMessageBatch"/>. This is separate from <see cref="BlueprintCustomisation{TDependency, TResult}"/>
/// because <c>CreateMessageBatchAsync</c> returns a <see cref="ValueTask{TResult}"/>.
/// </summary>
internal sealed class CreateMessageBatchCustomisation : IBlueprintCustomisation<ServiceBusSender>
{
    /// <summary>
    /// The maximum message size on the Standard tier of Service Bus.
    /// </summary>
    private const long MaxBatchSizeInBytes = 262_144;

    /// <inheritdoc />
    public void Apply(ServiceBusSender substitute)
    {
        ArgumentNullException.ThrowIfNull(substitute);

        substitute.CreateMessageBatchAsync(Arg.Any<CancellationToken>())
            .Returns(_ => CreateBatchAsync());

        substitute.CreateMessageBatchAsync(Arg.Any<CreateMessageBatchOptions>(), Arg.Any<CancellationToken>())
            .Returns(_ => CreateBatchAsync());
    }

    /// <summary>
    /// Creates a new batch for each call, as a real sender does, so messages do not accumulate across batches.
    /// </summary>
    /// <returns>An empty batch.</returns>
    private static ValueTask<ServiceBusMessageBatch> CreateBatchAsync()
    {
        var batch = ServiceBusModelFactory.ServiceBusMessageBatch(MaxBatchSizeInBytes, []);

        return new ValueTask<ServiceBusMessageBatch>(batch);
    }
}

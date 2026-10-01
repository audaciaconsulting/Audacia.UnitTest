using Audacia.UnitTest.Dependency.Blueprints;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.StorageQueue;

/// <summary>
/// A blueprint for a <see cref="QueueClient"/>.
/// This is the object for sending messages to a storage queue.
/// </summary>
public sealed class StorageQueueClientBlueprint : BlueprintDependency<QueueClient>
{
    private const int ServiceUnavailableStatus = 503;

    /// <summary>
    /// Creates a default <see cref="StorageQueueClientBlueprint"/> where any message sent will be accepted.
    /// </summary>
    public StorageQueueClientBlueprint()
    {
        var response = CreateSendResponse();

        foreach (var sendCall in SendCalls())
        {
            Customisations.Add(new BlueprintCustomisation<QueueClient, Response<SendReceipt>>(sendCall, response));
        }
    }

    /// <summary>
    /// Creates a <see cref="StorageQueueClientBlueprint"/> where any message sent will throw an exception.
    /// </summary>
    /// <returns>Blueprint configured to throw a <see cref="RequestFailedException"/> when sending a message.</returns>
    public static StorageQueueClientBlueprint ThrowExceptionWhenSending()
    {
        var blueprint = new StorageQueueClientBlueprint();

        foreach (var sendCall in SendCalls())
        {
            blueprint.Customisations.Add(
                new BlueprintCustomisation<QueueClient, Response<SendReceipt>>(
                    sendCall,
                    CreateFailure()));
        }

        return blueprint;
    }

    /// <summary>
    /// Every overload of <see cref="QueueClient"/> for sending a message, so behaviour applies however it is called.
    /// </summary>
    private static Func<QueueClient, Task<Response<SendReceipt>>>[] SendCalls()
    {
        return
        [
            client => client.SendMessageAsync(Arg.Any<string>()),
            client => client.SendMessageAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()),
            client => client.SendMessageAsync(
                Arg.Any<string>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>()),
            client => client.SendMessageAsync(
                Arg.Any<BinaryData>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>())
        ];
    }

    private static RequestFailedException CreateFailure()
    {
        return new RequestFailedException(ServiceUnavailableStatus, "The storage queue is unavailable.");
    }

    private static Response<SendReceipt> CreateSendResponse()
    {
        var receipt = QueuesModelFactory.SendReceipt(
            "message-id",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            "pop-receipt",
            DateTimeOffset.UtcNow);

        return Response.FromValue(receipt, Substitute.For<Response>());
    }
}

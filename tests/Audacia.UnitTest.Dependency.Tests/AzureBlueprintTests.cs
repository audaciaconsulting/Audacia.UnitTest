#pragma warning disable IDISP013 // Batches are awaited from the blueprint under test and disposed by their using.
using Audacia.Azure.BlobStorage.AddBlob;
using Audacia.Azure.BlobStorage.AddBlob.Commands;
using Audacia.Azure.BlobStorage.DeleteBlob.Commands;
using Audacia.UnitTest.Dependency.Azure.ServiceBus;
using Audacia.UnitTest.Dependency.Azure.Storage;
using Audacia.UnitTest.Dependency.Azure.StorageQueue;
using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

/// <summary>
/// Covers the blueprints in <c>Audacia.UnitTest.Dependency.Azure</c>, so each fake client
/// accepts or rejects a send in the way its name says it will.
/// </summary>
public class AzureBlueprintTests
{
    private const string ClientName = "orders";
    private const string EntityName = "order-placed";

    [Fact]
    public async Task Should_accept_a_message_from_any_service_bus_sender_by_default()
    {
        // Arrange
        var factory = new AzureClientFactoryServiceBusBlueprint().Build();

        // Act
        var sender = factory.CreateClient("any-client").CreateSender("any-entity");
        var send = () => sender.SendMessageAsync(new ServiceBusMessage("hello"));

        // Assert
        await send.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Should_accept_a_message_for_the_named_service_bus_client_and_entity()
    {
        // Arrange
        var factory = AzureClientFactoryServiceBusBlueprint.Create(ClientName, EntityName).Build();

        // Act
        var sender = factory.CreateClient(ClientName).CreateSender(EntityName);
        var send = () => sender.SendMessageAsync(new ServiceBusMessage("hello"));

        // Assert
        await send.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Should_throw_when_sending_to_the_named_service_bus_entity_created_with_failure()
    {
        // Arrange
        var factory = AzureClientFactoryServiceBusBlueprint.CreateWithFailure(ClientName, EntityName).Build();
        var sender = factory.CreateClient(ClientName).CreateSender(EntityName);
        var message = new ServiceBusMessage("hello");
        using var batch = ServiceBusModelFactory.ServiceBusMessageBatch(long.MaxValue, [message]);

        // Act & Assert
        foreach (var send in SendUsingEveryMethod(sender, message, batch))
        {
            await send.ShouldThrowAsync<ServiceBusException>();
        }
    }

    [Fact]
    public async Task Should_throw_when_a_service_bus_sender_is_built_to_throw_when_sending()
    {
        // Arrange
        var sender = ServiceBusSenderBlueprint.ThrowExceptionWhenSending().Build();

        // Act
        var send = () => sender.SendMessageAsync(new ServiceBusMessage("hello"));

        // Assert
        await send.ShouldThrowAsync<ServiceBusException>();
    }

    [Fact]
    public async Task Should_accept_a_message_sent_or_scheduled_using_any_service_bus_sender_method()
    {
        // Arrange
        var sender = new ServiceBusSenderBlueprint().Build();
        var message = new ServiceBusMessage("hello");
        using var batch = ServiceBusModelFactory.ServiceBusMessageBatch(long.MaxValue, [message]);

        // Act & Assert
        foreach (var send in SendUsingEveryMethod(sender, message, batch))
        {
            await send.ShouldNotThrowAsync();
        }
    }

    [Fact]
    public async Task Should_throw_when_a_message_is_sent_or_scheduled_using_any_service_bus_sender_method()
    {
        // Arrange
        var sender = ServiceBusSenderBlueprint.ThrowExceptionWhenSending().Build();
        var message = new ServiceBusMessage("hello");
        using var batch = ServiceBusModelFactory.ServiceBusMessageBatch(long.MaxValue, [message]);

        // Act & Assert
        foreach (var send in SendUsingEveryMethod(sender, message, batch))
        {
            await send.ShouldThrowAsync<ServiceBusException>();
        }
    }

    [Fact]
    public async Task Should_create_a_batch_that_a_default_service_bus_sender_accepts()
    {
        // Arrange
        var sender = new ServiceBusSenderBlueprint().Build();

        // Act
        using var batch = await sender.CreateMessageBatchAsync();
        var added = batch.TryAddMessage(new ServiceBusMessage("hello"));
        var send = () => sender.SendMessagesAsync(batch);

        // Assert
        added.ShouldBeTrue();
        batch.Count.ShouldBe(1);
        await send.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Should_create_a_new_batch_for_each_call_to_create_a_message_batch()
    {
        // Arrange
        var sender = new ServiceBusSenderBlueprint().Build();

        // Act
        using var first = await sender.CreateMessageBatchAsync();
        using var second = await sender.CreateMessageBatchAsync(new CreateMessageBatchOptions());
        first.TryAddMessage(new ServiceBusMessage("hello"));

        // Assert
        second.ShouldNotBeSameAs(first);
        second.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Should_throw_when_sending_a_batch_created_by_a_service_bus_sender_built_to_fail()
    {
        // Arrange
        var sender = ServiceBusSenderBlueprint.ThrowExceptionWhenSending().Build();

        // Act
        using var batch = await sender.CreateMessageBatchAsync();
        var send = () => sender.SendMessagesAsync(batch);

        // Assert
        await send.ShouldThrowAsync<ServiceBusException>();
    }

    [Fact]
    public async Task Should_return_a_sequence_number_when_scheduling_with_a_default_service_bus_sender()
    {
        // Arrange
        var sender = new ServiceBusSenderBlueprint().Build();

        // Act
        var single = await sender.ScheduleMessageAsync(new ServiceBusMessage("hello"), DateTimeOffset.UtcNow);
        var multiple = await sender.ScheduleMessagesAsync([new ServiceBusMessage("hello")], DateTimeOffset.UtcNow);

        // Assert
        single.ShouldBe(1);
        multiple.ShouldBe([1L]);
    }

    [Fact]
    public async Task Should_return_the_given_service_bus_sender_for_its_entity_name()
    {
        // Arrange
        var failingSender = ServiceBusSenderBlueprint.ThrowExceptionWhenSending().Build();
        var client = ServiceBusClientBlueprint.WithNamedSender(EntityName, failingSender).Build();

        // Act
        var namedSender = client.CreateSender(EntityName);
        var otherSender = client.CreateSender("another-entity");

        // Assert
        namedSender.ShouldBeSameAs(failingSender);
        await otherSender.SendMessageAsync(new ServiceBusMessage("hello"));
    }

    [Fact]
    public void Should_return_the_given_service_bus_client_for_the_given_client_name()
    {
        // Arrange
        var client = new ServiceBusClientBlueprint().Build();
        var factory = new AzureClientFactoryServiceBusBlueprint(ClientName, client).Build();

        // Act
        var named = factory.CreateClient(ClientName);
        var other = factory.CreateClient("another-client");

        // Assert
        named.ShouldBeSameAs(client);
        other.ShouldNotBeSameAs(client);
    }

    [Fact]
    public void Should_return_the_given_service_bus_client_for_each_of_the_given_client_names()
    {
        // Arrange
        var client = new ServiceBusClientBlueprint().Build();
        var factory = new AzureClientFactoryServiceBusBlueprint(["orders", "billing"], client).Build();

        // Act
        var orders = factory.CreateClient("orders");
        var billing = factory.CreateClient("billing");
        var other = factory.CreateClient("another-client");

        // Assert
        orders.ShouldBeSameAs(client);
        billing.ShouldBeSameAs(client);
        other.ShouldNotBeSameAs(client);
    }

    [Fact]
    public async Task Should_accept_a_message_from_any_storage_queue_by_default()
    {
        // Arrange
        var factory = new AzureClientFactoryStorageQueueBlueprint().Build();

        // Act
        var queue = factory.CreateClient("any-client").GetQueueClient("any-queue");
        var send = () => queue.SendMessageAsync("hello", CancellationToken.None);

        // Assert
        await send.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Should_accept_a_message_for_the_named_storage_queue_client_and_queue()
    {
        // Arrange
        var factory = AzureClientFactoryStorageQueueBlueprint.Create(ClientName, EntityName).Build();

        // Act
        var queue = factory.CreateClient(ClientName).GetQueueClient(EntityName);
        var send = () => queue.SendMessageAsync("hello", CancellationToken.None);

        // Assert
        await send.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Should_throw_when_sending_to_the_named_storage_queue_created_with_failure()
    {
        // Arrange
        var factory = AzureClientFactoryStorageQueueBlueprint.CreateWithFailure(ClientName, EntityName).Build();
        var queue = factory.CreateClient(ClientName).GetQueueClient(EntityName);

        // Act & Assert
        foreach (var send in SendUsingEveryOverload(queue))
        {
            await send.ShouldThrowAsync<RequestFailedException>();
        }
    }

    [Fact]
    public async Task Should_throw_when_a_storage_queue_client_is_built_to_throw_when_sending()
    {
        // Arrange
        var queue = StorageQueueClientBlueprint.ThrowExceptionWhenSending().Build();

        // Act
        var send = () => queue.SendMessageAsync("hello", CancellationToken.None);

        // Assert
        await send.ShouldThrowAsync<RequestFailedException>();
    }

    [Fact]
    public async Task Should_return_the_given_storage_queue_client_for_its_queue_name()
    {
        // Arrange
        var failingQueue = StorageQueueClientBlueprint.ThrowExceptionWhenSending().Build();
        var client = StorageQueueServiceClientBlueprint.WithNamedQueue(EntityName, failingQueue).Build();

        // Act
        var namedQueue = client.GetQueueClient(EntityName);
        var otherQueue = client.GetQueueClient("another-queue");

        // Assert
        namedQueue.ShouldBeSameAs(failingQueue);
        await otherQueue.SendMessageAsync("hello", CancellationToken.None);
    }

    [Fact]
    public void Should_return_the_given_storage_queue_service_client_for_the_given_client_name()
    {
        // Arrange
        var client = new StorageQueueServiceClientBlueprint().Build();
        var factory = new AzureClientFactoryStorageQueueBlueprint(ClientName, client).Build();

        // Act
        var named = factory.CreateClient(ClientName);
        var other = factory.CreateClient("another-client");

        // Assert
        named.ShouldBeSameAs(client);
        other.ShouldNotBeSameAs(client);
    }

    [Fact]
    public async Task Should_accept_a_message_sent_using_any_storage_queue_send_overload()
    {
        // Arrange
        var queue = new StorageQueueClientBlueprint().Build();

        // Act & Assert
        foreach (var send in SendUsingEveryOverload(queue))
        {
            await send.ShouldNotThrowAsync();
        }
    }

    [Fact]
    public async Task Should_return_a_receipt_for_a_message_sent_using_any_storage_queue_send_overload()
    {
        // Arrange
        var queue = new StorageQueueClientBlueprint().Build();

        // Act & Assert
        foreach (var send in SendUsingEveryOverload(queue))
        {
            var response = await send();

            response.ShouldNotBeNull();
            response.Value.ShouldNotBeNull();
            response.Value.MessageId.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Should_throw_when_a_message_is_sent_using_any_storage_queue_send_overload()
    {
        // Arrange
        var queue = StorageQueueClientBlueprint.ThrowExceptionWhenSending().Build();

        // Act & Assert
        foreach (var send in SendUsingEveryOverload(queue))
        {
            await send.ShouldThrowAsync<RequestFailedException>();
        }
    }

    [Fact]
    public async Task Should_report_a_blob_as_added_by_default()
    {
        // Arrange
        var service = new AddAzureBlobStorageServiceBlueprint().Build();

        // Act
        var added = await service.ExecuteAsync(new AddBlobBytesCommand("documents", "report.pdf", [1, 2, 3]), CancellationToken.None);

        // Assert
        added.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_report_a_blob_as_deleted_by_default()
    {
        // Arrange
        var service = new DeleteAzureBlobStorageServiceBlueprint().Build();

        // Act
        var deleted = await service.ExecuteAsync(new DeleteAzureBlobStorageCommand("documents", "report.pdf"), CancellationToken.None);

        // Assert
        deleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_report_a_blob_as_added_using_any_add_overload()
    {
        // Arrange
        var service = new AddAzureBlobStorageServiceBlueprint().Build();

        await using var stream = new MemoryStream([1, 2, 3]);

        // Act & Assert
        foreach (var add in AddUsingEveryOverload(service, stream))
        {
            (await add()).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Should_throw_when_a_blob_is_added_using_any_add_overload()
    {
        // Arrange
        var service = AddAzureBlobStorageServiceBlueprint.ThrowExceptionWhenAdding().Build();

        await using var stream = new MemoryStream([1, 2, 3]);

        // Act & Assert
        foreach (var add in AddUsingEveryOverload(service, stream))
        {
            await add().ShouldThrowAsync<RequestFailedException>();
        }
    }

    [Fact]
    public async Task Should_throw_when_a_blob_is_deleted()
    {
        // Arrange
        var service = DeleteAzureBlobStorageServiceBlueprint.ThrowExceptionWhenDeleting().Build();

        // Act & Assert
        await service.ExecuteAsync(new DeleteAzureBlobStorageCommand("documents", "report.pdf"), CancellationToken.None)
            .ShouldThrowAsync<RequestFailedException>();
    }

    private static Func<Task<bool>>[] AddUsingEveryOverload(IAddAzureBlobStorageService service, Stream stream)
    {
        return
        [
            () => service.ExecuteAsync(new AddBlobBytesCommand("documents", "report.pdf", [1, 2, 3]), CancellationToken.None),
            () => service.ExecuteAsync(new AddBlobBaseSixtyFourCommand("documents", "report.pdf", "AQID"), CancellationToken.None),
            () => service.ExecuteAsync(new AddBlobFileCommand("documents", "report.pdf", "report.pdf"), CancellationToken.None),
            () => service.ExecuteAsync(new AddBlobStreamCommand("documents", "report.pdf", stream), CancellationToken.None)
        ];
    }

    private static Func<Task>[] SendUsingEveryMethod(
        ServiceBusSender sender,
        ServiceBusMessage message,
        ServiceBusMessageBatch batch)
    {
        return
        [
            () => sender.SendMessageAsync(message),
            () => sender.SendMessagesAsync([message]),
            () => sender.SendMessagesAsync(batch),
            () => sender.ScheduleMessageAsync(message, DateTimeOffset.UtcNow),
            () => sender.ScheduleMessagesAsync([message], DateTimeOffset.UtcNow)
        ];
    }

    private static Func<Task<Response<SendReceipt>>>[] SendUsingEveryOverload(QueueClient queue)
    {
        return
        [
            () => queue.SendMessageAsync("hello"),
            () => queue.SendMessageAsync("hello", CancellationToken.None),
            () => queue.SendMessageAsync("hello", null, null, CancellationToken.None),
            () => queue.SendMessageAsync(BinaryData.FromString("hello"), null, null, CancellationToken.None)
        ];
    }
}

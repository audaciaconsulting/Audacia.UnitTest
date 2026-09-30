# Audacia.UnitTest.Dependency.Azure

Blueprints that give a test target fake Azure dependencies — Service Bus, Storage Queues and Blob Storage — so a
class that sends messages or stores blobs can be tested without an Azure account. Companion package to
[`Audacia.UnitTest.Dependency`](../Audacia.UnitTest.Dependency).

Built on [NSubstitute](https://nsubstitute.github.io/).

The Blob Storage blueprints fake services from `Audacia.Azure.BlobStorage`, so this package references it, and its
dependencies come with the package even if you only use Service Bus or Storage Queues.

## Installation

```
dotnet add package Audacia.UnitTest.Dependency.Azure
```

## Getting started

Types such as `IAzureClientFactory<ServiceBusClient>` have no implementation of yours for the builder to find, so
supply a blueprint for them. With no arguments, every client name resolves to a client that accepts any message:

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(new AzureClientFactoryServiceBusBlueprint())
    .Build<OrderPlacedPublisher>();

await target.PublishAsync(new OrderPlaced(42));
```

### Discovering the blueprints automatically

Blueprints are only discovered in the assemblies your test project declares. Add this package to that list and the
blueprints below are used without passing them to `WithBlueprint`:

```csharp
[assembly: BlueprintAssembly("Audacia.UnitTest.Dependency.Azure")]
```

The attribute can be applied more than once. Note that once you declare any assembly, only the declared assemblies
are searched, so list your own blueprint assembly alongside this one.

## Available blueprints

| Blueprint | Provides |
| --- | --- |
| `AzureClientFactoryServiceBusBlueprint` | `IAzureClientFactory<ServiceBusClient>` |
| `ServiceBusClientBlueprint` | `ServiceBusClient` |
| `ServiceBusSenderBlueprint` | `ServiceBusSender` |
| `AzureClientFactoryStorageQueueBlueprint` | `IAzureClientFactory<QueueServiceClient>` |
| `StorageQueueServiceClientBlueprint` | `QueueServiceClient` |
| `StorageQueueClientBlueprint` | `QueueClient` |
| `AddAzureBlobStorageServiceBlueprint` | `IAddAzureBlobStorageService` (from `Audacia.Azure.BlobStorage`) |
| `DeleteAzureBlobStorageServiceBlueprint` | `IDeleteAzureBlobStorageService` (from `Audacia.Azure.BlobStorage`) |

Use the blueprint for whichever type your target takes. The factory blueprints are for code that resolves clients by
name through `IAzureClientFactory<T>`; the client blueprints are for code that takes the client directly.

## Service Bus

### Accepting every message

The parameterless blueprints accept anything. Any client name resolves to a client, and any entity name resolves to a
sender whose `SendMessageAsync` completes successfully:

```csharp
new AzureClientFactoryServiceBusBlueprint()
new ServiceBusClientBlueprint()
new ServiceBusSenderBlueprint()
```

The sender accepts `SendMessageAsync`, both `SendMessagesAsync` overloads (a collection or a batch) and
`ScheduleMessageAsync`/`ScheduleMessagesAsync`, so the target can use whichever it needs. `CreateMessageBatchAsync`
returns a new, empty batch on each call that messages can be added to and sent. The failure blueprints below throw
from every send and schedule method, but still create batches, so the failure is seen when the batch is sent.

### Naming the client and entity

When the code under test requests a specific client and queue/topic, use `Create`:

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(AzureClientFactoryServiceBusBlueprint.Create("orders", "order-placed"))
    .Build<OrderPlacedPublisher>();
```

### Simulating a failure

`CreateWithFailure` returns a sender that throws a `ServiceBusException` for every message, for testing how the
target handles a failed send:

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(AzureClientFactoryServiceBusBlueprint.CreateWithFailure("orders", "order-placed"))
    .Build<OrderPlacedPublisher>();

await Should.ThrowAsync<ServiceBusException>(() => target.PublishAsync(new OrderPlaced(42)));
```

To fail at the sender level, use `ServiceBusSenderBlueprint.ThrowExceptionWhenSending()`.

### Supplying your own client

If you already have a `ServiceBusClient`, register it against one name or several:

```csharp
new AzureClientFactoryServiceBusBlueprint("orders", client)
new AzureClientFactoryServiceBusBlueprint(new[] { "orders", "billing" }, client)
```

To map an entity name to a specific sender, use `ServiceBusClientBlueprint.WithNamedSender(entityName, sender)`.

## Storage Queues

The Storage Queue blueprints mirror the Service Bus ones, with `QueueServiceClient` in place of `ServiceBusClient`
and `QueueClient` in place of `ServiceBusSender`:

```csharp
// Any client name, any queue: messages are accepted.
new AzureClientFactoryStorageQueueBlueprint()

// A named client and queue: messages are accepted.
AzureClientFactoryStorageQueueBlueprint.Create("notifications", "email-queue")

// A named client and queue: sending throws a RequestFailedException.
AzureClientFactoryStorageQueueBlueprint.CreateWithFailure("notifications", "email-queue")

// Your own QueueServiceClient for a client name.
new AzureClientFactoryStorageQueueBlueprint("notifications", queueServiceClient)
```

For code that takes the client directly, `StorageQueueServiceClientBlueprint.WithNamedQueue(queueName, queueClient)`
maps a queue name to a specific `QueueClient`, and `StorageQueueClientBlueprint.ThrowExceptionWhenSending()` gives a
queue client that fails.

## Blob Storage

These blueprints fake the `Audacia.Azure.BlobStorage` services. Every `ExecuteAsync` call completes and returns
`true`. The add blueprint covers every way of adding a blob (bytes, base 64, a file or a stream):

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(new AddAzureBlobStorageServiceBlueprint())
    .WithBlueprint(new DeleteAzureBlobStorageServiceBlueprint())
    .Build<DocumentService>();
```

To make adding or deleting fail, use the failure blueprints, which throw an `Azure.RequestFailedException`, as the Storage
Queue ones do:

```csharp
AddAzureBlobStorageServiceBlueprint.ThrowExceptionWhenAdding()
DeleteAzureBlobStorageServiceBlueprint.ThrowExceptionWhenDeleting()
```

## Asserting on what was sent

The blueprints build NSubstitute substitutes, so to check what your target sent, keep hold of the client and
inspect it with NSubstitute's `Received`. Build the blueprint yourself, register the built instance, then assert:

```csharp
var sender = new ServiceBusSenderBlueprint().Build();
var client = ServiceBusClientBlueprint.WithNamedSender("order-placed", sender).Build();

var target = new TestTargetBuilder()
    .WithBlueprint(new AzureClientFactoryServiceBusBlueprint("orders", client))
    .Build<OrderPlacedPublisher>();

await target.PublishAsync(new OrderPlaced(42));

await sender.Received(1).SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>());
```

## Customising a blueprint

The base class for these blueprints is `CustomisedBlueprintDependency<T>`. Derive from it, or from
`AzureClientFactoryBlueprint<TClient>` for an `IAzureClientFactory<TClient>`, to make a blueprint for a client type
not covered above. Add a `BlueprintCustomisation` for each call you want to configure; the factory base class also
provides `SetupNamedClient`, `SetupNamedClients` and `SetupAnyNamedClient` for registering clients by name.

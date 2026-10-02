# Audacia.UnitTest.Dependency.Mediator

Lets a test target that depends on `IMediator` from [`Audacia.Mediator`](https://github.com/audaciaconsulting/Audacia.Mediator)
send requests through the real mediator pipeline. Handlers, pipeline behaviours and validators are built by the
`TestTargetBuilder`, so the services they depend on are supplied in the same way as for any other test target.
Companion package to [`Audacia.UnitTest.Dependency`](../Audacia.UnitTest.Dependency).

## Installation

```
dotnet add package Audacia.UnitTest.Dependency.Mediator
```

## Getting started

Pass the assemblies containing your handlers. The handlers are found with the same scan `AddMediator` uses in the
application, and are built with whatever you supply to the builder:

```csharp
var service = new TestTargetBuilder()
    .WithOptions(new OrderOptions { MaxLines = 5 })
    .With<IOrderRepository>(repository)
    .WithMediator(typeof(CreateOrderHandler).Assembly)
    .Build<OrderService>();   // OrderService depends on IMediator
```

Dependencies can be supplied before or after `WithMediator`, and so can pipeline behaviours (see below). The mediator is
created when the first request is sent, so everything must be added before then. `WithMediator` can be called once per
builder, and accepts several assemblies.

## Pipeline behaviours

Behaviours run in the order they are added, with the first one added outermost. Add any class implementing
`IPipelineBehavior<TRequest, TResponse>`, including an open generic. It is built by the `TestTargetBuilder`, so its own
constructor dependencies are resolved or supplied in the usual way:

```csharp
.WithMediator(typeof(CreateOrderHandler).Assembly)
.AddPipelineBehavior(typeof(LoggingBehavior<,>))
```

An open generic behaviour applies to every request whose types satisfy its constraints, as it does in the application.

For a quick step in a test without writing a class, add a delegate for a particular request:

```csharp
.AddPipelineBehavior<CreateOrder, ErrorOr<int>>(async (request, next, token) =>
{
    log.Add("before");
    return await next(token);
})
```

## Validators

The consuming project supplies its own validation behaviour, as it does in the application. Validators do not need to be
registered: when a behaviour depends on `IEnumerable<T>` of an interface, such as `IEnumerable<IValidator<TRequest>>`,
the implementations of that interface found in the handler assemblies are supplied automatically. They are built by the
`TestTargetBuilder` only when a request that needs them is sent, so anything they depend on can be supplied with `With`,
and no validation library is assumed:

```csharp
.WithMediator(typeof(CreateOrderHandler).Assembly)
.AddPipelineBehavior(typeof(ValidationBehavior<,>))
```

If a test supplies its own `IEnumerable<IValidator<CreateOrder>>` with `With`, that is used instead. Validators in an
assembly that has no handlers are not found; pass that assembly to `WithMediator` as well.

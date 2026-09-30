# Audacia.UnitTest.Dependency.DependencyInjection

Lets a `TestTargetBuilder` use your project's own service registration — for example an `AddApplication` extension
method — so a test target gets the services the container provides, while everything else it depends on is still built
by the builder. Companion package to [`Audacia.UnitTest.Dependency`](../Audacia.UnitTest.Dependency).

Some dependencies cannot be built by scanning for an implementation: an interface whose implementation is `internal`, or
a service such as `IMediator` that only works when a container has registered the handlers it dispatches to. This package
tells the builder how your project registers those services, once, so tests do not repeat it.

## Installation

```
dotnet add package Audacia.UnitTest.Dependency.DependencyInjection
```

## Getting started

Add one class to the test project. It is found automatically, in the same way as a blueprint:

```csharp
public sealed class ApplicationServiceRegistration : ServiceRegistration
{
    protected override void Register(IServiceCollection services)
    {
        services.AddApplication();
    }
}
```

Every test target can now depend on anything `AddApplication` registers:

```csharp
var mediator = new TestTargetBuilder().Build<IMediator>();
var handler = new TestTargetBuilder().Build<GetOrderQueryHandler>();  // takes an IMediator, for example
```

The class needs a parameterless constructor and must be public. Like blueprints, it is looked for in the test project, or in
the assemblies named by `[assembly: BlueprintAssembly("...")]`.

## What comes from the container, and what from the builder

For each type a test target needs, the builder works down its usual list (see
[`Audacia.UnitTest.Dependency`](../Audacia.UnitTest.Dependency/README.md#how-a-dependency-is-resolved)). Registrations
sit just after blueprints:

1. An instance registered with `With`, `WithOptions` or `WithOptionsSnapshot`.
2. A blueprint.
3. **A service registered by a `ServiceRegistration`** (or `WithServices`) — built by the container.
4. Anything else, constructed by the builder as normal.

Services the container builds may depend on things the registration did **not** add — a database context, an `HttpClient`, a
clock. Those are built by the builder, using the same rules, so `With` and `WithBlueprint` replace them exactly as they do
for any other test target:

```csharp
var database = Substitute.For<IDatabaseContext>();

var mediator = new TestTargetBuilder()
    .With(database)   // the handlers that AddApplication registered receive this
    .Build<IMediator>();
```

This is worked out when a service is first needed, so `With` and `WithBlueprint` can be called at any point before then.

## A registration for a single test

Use `WithServices` when a test needs different registrations, or a handler that exists only in the test project. It
takes precedence over any registration found automatically:

```csharp
var mediator = new TestTargetBuilder()
    .WithServices(services => services.AddApplication().AddMediator(typeof(TestOnlyQuery).Assembly))
    .Build<IMediator>();
```

## Using this with Audacia.Mediator

[Audacia.Mediator](https://www.nuget.org/packages/Audacia.Mediator) finds handlers through the container, so tests that go
through `IMediator` need the registration above. Tests of a single handler or validator do not:

| To test | Use | Notes |
| --- | --- | --- |
| A handler on its own | `Build<MyCommandHandler>()` | Its constructor dependencies are built as normal. A handler that takes an `IMediator` gets the real one from your registration. |
| A validator on its own | `Build<MyCommandValidator>()` | Validators usually have no dependencies. |
| The whole pipeline — validation, then the handler | `Build<IMediator>()`, then `SendAsync` | Uses the real pipeline behaviors and validators registered by `AddApplication`. |

```csharp
[Fact]
public async Task A_price_below_the_minimum_is_a_validation_failure()
{
    var mediator = new TestTargetBuilder().Build<IMediator>();

    var result = await mediator.SendAsync(CommandWithPrice(0m), TestContext.Current.CancellationToken);

    result.FirstError.Type.ShouldBe(ErrorType.Validation);
}
```

Do not register the handlers a second time: `AddApplication` (or whatever your project calls `AddMediator`) is the one
place that knows which handlers and validators exist, so a new handler is picked up by tests with no further change.

Dependencies of handlers that talk to something outside the process — a database, an API, Azure — are the ones to
replace. Give the test project a blueprint for each (for example `IDatabaseContext`), and the tests get it automatically;
see [`Audacia.UnitTest.Dependency.Http`](../Audacia.UnitTest.Dependency.Http/README.md) and
[`Audacia.UnitTest.Dependency.Azure`](../Audacia.UnitTest.Dependency.Azure/README.md) for ready-made ones.

## Seeing what was used

`builder.DescribeResolutions()` shows which dependencies the builder supplied automatically, including services that came
from a registration, with the concrete type used for each. See
[Seeing what was used](../Audacia.UnitTest.Dependency/README.md#seeing-what-was-used).

## Notes

- Registered services come from a single shared scope for each builder, so scoped services behave as they would within one
  request.
- That scope is not disposed, so avoid registering services that hold unmanaged resources.
- Open generic registrations (such as a pipeline behavior) are used by the container. Their own constructor dependencies
  are not inspected, and when a registered service needs a closed form of one (for example `IBehavior<Order>`), the builder
  supplies it by scanning rather than the container, so register or supply anything such a type depends on with `With` or
  `WithBlueprint`.
- The builder is only asked for the dependencies in the constructor the container uses: the public constructor with the
  most parameters. Services registered with a factory or an instance (for example `AddDbContext`) are not inspected, so
  what they need must be registered too.
- If a registered service needs a type that the builder constructs, and that type in turn needs the registered service, the
  cycle is reported with a `TestTargetBuilderException` naming the type. Register the type, or give it to the builder with
  `With` or `WithBlueprint`, to break the cycle.
- If more than one registration can supply a type, the first wins: those added with `WithServices`, in the order added,
  then those found automatically.

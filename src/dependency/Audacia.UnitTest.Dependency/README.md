# Audacia.UnitTest.Dependency

`Audacia.UnitTest.Dependency` builds the class under test — the *test target* — and resolves its entire dependency
graph for you. By default every dependency is a real instance, constructed recursively, so a test exercises real
collaborators unless you deliberately replace one.

## Installation

```
dotnet add package Audacia.UnitTest.Dependency
```

## Getting started

Build the target and its dependencies are resolved automatically:

```csharp
var target = new TestTargetBuilder().Build<AddPersonCommandHandler>();

var result = await target.HandleAsync(new AddPersonCommand("Joe Bloggs"));
```

Nothing needs registering up front. `AddPersonCommandHandler` takes an `IValidatePersonCommandHandler` and an
`ILogger<AddPersonCommandHandler>`; the builder finds your implementation of the first and supplies a no-op logger
for the second.

## How a dependency is resolved

For each type in the graph the builder works down this list and takes the first that applies:

1. **An instance you registered** with `With`, `WithOptions` or `WithOptionsSnapshot`.
2. **A blueprint** for the type — one registered with `WithBlueprint`, or one discovered automatically.
3. **A class** — constructed via its first constructor, resolving each parameter the same way.
4. **`IOptions<T>`** — wrapped in an `OptionsWrapper<T>`.
5. **`ILogger<T>`** — supplied as a `NullLogger<T>`.
6. **An interface** — resolved to an implementation found in the assemblies in scope.

If none apply, a `TestTargetBuilderException` is thrown naming the type and the chain of types being constructed.

Step 6 only searches your own assemblies, so a framework interface with no implementation of yours — such as
`IHttpClientFactory` — needs a blueprint or a registered instance. See [Faking HTTP](#faking-http).

## Replacing a dependency

### A specific instance

Use `With` when you want one collaborator to behave a particular way. The type is inferred from the argument, or
state it explicitly to register against an interface:

```csharp
var validator = Substitute.For<IValidatePersonCommandHandler>();
validator.HandleAsync(Arg.Any<ValidatePersonCommand>())
    .Returns(CommandResult.Failure("Validation failed"));

var target = new TestTargetBuilder()
    .With(validator)
    .Build<AddPersonCommandHandler>();
```

Pass the substitute itself, not a mock wrapper around it. Passing a Moq `Mock<T>`, or a blueprint that belongs in
`WithBlueprint`, throws with an explanation rather than failing later.

### Configuration

```csharp
var target = new TestTargetBuilder()
    .WithOptions(new AppConfiguration { RetryCount = 3 })
    .Build<AddPersonCommandHandler>();
```

The instance is used live, so changing it mid-test changes what the target sees.

For named configuration, use `WithOptionsSnapshot`. The target receives an `IOptionsSnapshot<T>` whose `Get(name)`
returns the matching entry:

```csharp
var configurations = new Dictionary<string, AppConfiguration>
{
    ["primary"] = new() { RetryCount = 1 },
    ["secondary"] = new() { RetryCount = 5 }
};

var target = new TestTargetBuilder()
    .WithOptionsSnapshot(configurations)
    .Build<AddPersonCommandHandler>();
```

The second argument sets the value used by `.Value` and by any name that was not configured. Without it, the first
entry is used.

## Blueprints

A blueprint describes how to build one dependency, so the same setup can be shared across tests instead of repeated.
Derive from `BlueprintDependency<TDependency>`:

```csharp
public class PersonStoreBlueprint : BlueprintDependency<IPersonStore>
{
    public override IPersonStore Build()
    {
        var store = Substitute.For<IPersonStore>();
        store.Exists(Arg.Any<string>()).Returns(true);

        return store;
    }
}
```

Implementing `IBlueprintDependency<TDependency>` directly works too, which is useful when the blueprint already has
a base class of its own.

Blueprints are discovered automatically, so the one above is used for any `IPersonStore` in the graph without being
registered. To use a specific instance for a single test, pass it in:

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(new PersonStoreBlueprint())
    .Build<AddPersonCommandHandler>();
```

### Where blueprints are looked for

By default the test project itself. If your blueprints live elsewhere, point at those assemblies — the attribute
can be applied more than once:

```csharp
[assembly: BlueprintAssembly("MyProduct.Tests.Blueprints")]
```

## Generic dependencies

Open generic implementations are closed using the generic arguments of the interface being resolved. A target
depending on `IEntityRepository<Person>` resolves to `EntityRepository<Person>` without any registration, given:

```csharp
public class EntityRepository<TEntity> : IEntityRepository<TEntity>
    where TEntity : class;
```

Generic blueprints work the same way — a blueprint for an open generic dependency is closed over the requested
type arguments.

## Narrowing the assemblies searched

The builder searches the test assembly and every assembly it references, directly or transitively, whose name
starts with the same first namespace segment. To skip some of those, list them when constructing the builder:

```csharp
var target = new TestTargetBuilder(["MyProduct.Legacy"])
    .Build<AddPersonCommandHandler>();
```

## Building a target known only at runtime

```csharp
var target = builder.Build(typeof(AddPersonCommandHandler));
```

## When resolution fails

`TestTargetBuilderException` reports the type that could not be built along with the chain that led to it, so the
failure can be traced back to the target:

```
Could not construct a service for IPersonStore (constructing these types: AddPersonCommandHandler > IPersonStore)
```

Its `Service` property holds the name of the type that failed. A `BlueprintDependencyException` indicates a problem
with a blueprint itself, such as a marked assembly that could not be loaded.

## Faking HTTP

To give a target a fake `HttpClient` or `IHttpClientFactory`, install
[`Audacia.UnitTest.Dependency.Http`](../Audacia.UnitTest.Dependency.Http/README.md), which provides ready-made
blueprints for both along with a builder for canned API responses:

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(new HttpClientFactoryBlueprint())
    .Build<AddAssetCommandHandler>();
```

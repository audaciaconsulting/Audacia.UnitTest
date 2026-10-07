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
3. **A class** — constructed by the builder via its first public constructor, resolving each parameter the same way.
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

If using a substitute, pass in the substitute itself, not a wrapper around it. Passing in a `Mock<T>`, or a blueprint that belongs in
`WithBlueprint`, throws an error with an explanation (rather than failing later).

`With` isn't limited to substitutes — any manually constructed instance works, including a plain real object with
specific state, which keeps a test using a genuine dependency instead of a fake one:

```csharp
var target = new TestTargetBuilder()
    .With(new AppConfiguration { RetryCount = 3 })
    .Build<AddPersonCommandHandler>();
```

The difference from `WithOptions` below is what the target receives: `With` registers the value as its own type
(here, `AppConfiguration`), while `WithOptions` wraps it as `IOptions<AppConfiguration>`. Use whichever one matches
what the target actually depends on.

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

A blueprint creates one reusable dependency that allows the same setup to be shared across tests, instead of repeated. This handles 
dependencies 'on the edge of' your application, e.g. where it interacts with third parties, a database or Azure resources. 

Some packages are included in this solution - e.g. for Azure and HTTP - but you may need to add your own, for example a finance system 
for posting invoices, government API for bank holidays, or communication provider for sending email or SMS messages.

Implement `IBlueprintDependency<TDependency>` directly, or derive from `BlueprintDependency<TDependency>` instead
when the blueprint has no base class of its own (see [Customised blueprints for substitutes](#customised-blueprints-for-substitutes)):

```csharp
public sealed class PayrollApiBlueprint : IBlueprintDependency<IPayrollApi>
{
    public IPayrollApi Build()
    {
        return new PayrollApiClient();
    }
}
```

`PayrollApiClient` is `internal`, so the builder cannot find it by scanning for exported implementations — without a
blueprint, `IPayrollApi` could not be resolved at all. Blueprints are discovered automatically, so the one
above is used for any `IPayrollApi` in the graph without being registered:

```csharp
var target = new TestTargetBuilder().Build<SalaryPaymentGenerator>();

var payment = target.Generate("value"); // "blueprint:value"
```

To use a specific instance for a single test instead of the discovered blueprint, pass it in:

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(new PayrollApiBlueprint())
    .Build<SalaryPaymentGenerator>();
```

### Where blueprints are looked for

By default the test project itself. If your blueprints live elsewhere, point at those assemblies — the attribute
can be applied more than once:

```csharp
[assembly: BlueprintAssembly("MyProduct.Tests.Blueprints")]
```

### Customised blueprints for substitutes

When a dependency should be an NSubstitute substitute with some behaviour set up, you can derive from
`BlueprintDependency<TDependency>` instead of writing `Build` yourself. Add an
`IBlueprintCustomisation<TDependency>` to its `Customisations` collection for each call you want to configure;
`Build` creates the substitute and applies them in the order they were added:

```csharp
public sealed class PersonStoreBlueprint : BlueprintDependency<IPersonStore>
{
    public PersonStoreBlueprint()
    {
        Customisations.Add(new BlueprintCustomisation<IPersonStore, bool>(
            store => store.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()),
            true));
    }
}
```

`BlueprintCustomisation<TDependency, TResult>` takes the call to configure, written as a lambda against the
substitute using NSubstitute argument matchers, and what that call should do. It can either return a result or throw
an exception, and works for both synchronous methods and those returning `Task<TResult>`:

```csharp
// Returns a value.
new BlueprintCustomisation<IPersonStore, bool>(
    store => store.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()),
    true)

// Throws. For an async method the exception surfaces from the returned task.
new BlueprintCustomisation<IPersonStore, bool>(
    store => store.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()),
    new TimeoutException())
```

To reuse a blueprint with a variation, add to `Customisations` after construction, or expose a static factory that
builds the default and adds one more. Where two customisations match the same call, the one added last wins, so put
the general case first and the specific cases after it:

```csharp
public static PersonStoreBlueprint WhereExistenceCheckFails()
{
    var blueprint = new PersonStoreBlueprint();
    blueprint.Customisations.Add(new BlueprintCustomisation<IPersonStore, bool>(
        store => store.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()),
        new TimeoutException()));

    return blueprint;
}
```

Implement `IBlueprintCustomisation<TDependency>` yourself for anything an extension of `BlueprintCustomisation` does not cover, such
as configuring a method that returns nothing or setting up a property.

`MockDependency` holds the substitute the blueprint builds, so a test can assert on the calls it received with
`Received()`. Keep hold of the blueprint and pass it to `WithBlueprint`:

```csharp
var blueprint = new PersonStoreBlueprint();
var target = new TestTargetBuilder()
    .WithBlueprint(blueprint)
    .Build<AddPersonCommandHandler>();

await target.HandleAsync(new AddPersonCommand("Joe Bloggs"));

await blueprint.MockDependency.Received(1)
    .ExistsAsync("Joe Bloggs", Arg.Any<CancellationToken>());
```

`Build` applies the customisations to `MockDependency` and returns it, so it is the same instance every time.
`MockDependency` can also be set, to customise a substitute you have already created.

The Azure blueprints in [`Audacia.UnitTest.Dependency.Azure`](../Audacia.UnitTest.Dependency.Azure/README.md) are
built this way.

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

## Seeing what was used

Because dependencies are built for you, a test that passes or fails unexpectedly may have used one you were not thinking
of. `DescribeResolutions` lists what the builder chose automatically, with the full name of the concrete type it used and
how it chose it:

```csharp
var builder = new TestTargetBuilder();
var target = builder.Build<AddPersonCommandHandler>();

output.WriteLine(builder.DescribeResolutions());
```

```
  PersonStore -> constructed: MyProduct.Data.PersonStore
  IDatabaseContext -> blueprint: MyProduct.Tests.TestDatabaseContextBlueprint => MyProduct.Tests.TestDatabaseContext
  IClock -> implementation found by scanning: MyProduct.Common.SystemClock
  Also: 2 logger(s) that discard output, 0 default options
```

Each line gives the type that was needed, then how it was supplied — a blueprint discovered
automatically, a class the builder constructed itself, or an interface resolved to an implementation found by scanning —
and the concrete type used. A substitute is marked as such. Dependencies you supplied yourself, with `With`,
`WithOptions` or `WithBlueprint`, are left out, since you already know about them. Loggers and
default options are summarised in one line.

`builder.Resolutions` holds the same information as a list of `DependencyResolution` if you want to assert on it or format
it differently. Entries appear in the order they were completed, so a class comes after the dependencies it was built from.

Nothing is written unless you ask, so the output never adds noise to passing tests.

## When resolution fails

`TestTargetBuilderException` reports the type that could not be built along with the chain that led to it, so the
failure can be traced back to the target. It also lists what the builder had already chosen automatically, in the same
form as `DescribeResolutions`, so an unexpected choice earlier in the graph is visible next to the failure:

```
Could not construct a service for IPersonStore (constructing these types: AddPersonCommandHandler > IPersonStore)
Dependencies chosen automatically so far (those supplied with With, WithBlueprint and so on are not listed):
  PersonValidator -> constructed: MyProduct.Validation.PersonValidator
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

## Faking Azure

To give a target fake Azure Service Bus, Storage Queue or Blob Storage dependencies, install
[`Audacia.UnitTest.Dependency.Azure`](../Audacia.UnitTest.Dependency.Azure/README.md):

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(new AzureClientFactoryServiceBusBlueprint())
    .Build<OrderPlacedPublisher>();
```

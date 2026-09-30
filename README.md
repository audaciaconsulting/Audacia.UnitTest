# Overview

The `Audacia.UnitTest` repo contains multiple packages with each having a purpose of making unit test writing easier.

- Audacia.UnitTest.Dependency
- Audacia.UnitTest.Dependency.Http
- Audacia.UnitTest.Dependency.Azure
- Audacia.UnitTest.Dependency.DependencyInjection

## Audacia.UnitTest.Dependency

The purpose of `Audacia.UnitTest.Dependency` is to help engineers quickly configure and build the test target, with by default creating real instance of all dependencies of dependencies for the test target.

Customisation is provided out of the box in two ways.
1. Providing `BlueprintDependency<TDependency>` within the solution as a generic method.
1. Providing the `TestTargetBuilder` with the customised dependency on creation.

For more details on how to use `Audacia.UnitTest.Dependency` see the package [README](./src/dependency/Audacia.UnitTest.Dependency/README.md).

## Audacia.UnitTest.Dependency.Http

The purpose of `Audacia.UnitTest.Dependency.Http` is to provide blueprints for `HttpClient`, so a test target depending on `HttpClient` or `IHttpClientFactory` is given a fake instance with canned responses.

For more details on how to use `Audacia.UnitTest.Dependency.Http` see the package [README](./src/dependency/Audacia.UnitTest.Dependency.Http/README.md).

## Audacia.UnitTest.Dependency.Azure

The purpose of `Audacia.UnitTest.Dependency.Azure` is to provide blueprints for the Azure services at the outer edge of an application, so a test target that sends to Service Bus or a Storage Queue, or adds and deletes blobs, is given a fake instance that accepts every call, or can be made to fail, without needing an Azure account.

For more details on how to use `Audacia.UnitTest.Dependency.Azure` see the package [README](./src/dependency/Audacia.UnitTest.Dependency.Azure/README.md).

## Audacia.UnitTest.Dependency.DependencyInjection

The purpose of `Audacia.UnitTest.Dependency.DependencyInjection` is to let the `TestTargetBuilder` use a project's own service registration, such as an `AddApplication` extension method. The registration is declared once in the test project, and a test target then gets the services the container provides (for example `IMediator` and its handlers) while everything else it depends on is still built by the `TestTargetBuilder`. It includes guidance on using this with Audacia.Mediator.

For more details on how to use `Audacia.UnitTest.Dependency.DependencyInjection` see the package [README](./src/dependency/Audacia.UnitTest.Dependency.DependencyInjection/README.md).

# Contributing

We welcome contributions! Please feel free to check our [Contribution Guidelines](https://github.com/audaciaconsulting/.github/blob/main/CONTRIBUTING.md) for feature requests, issue reporting and guidelines.

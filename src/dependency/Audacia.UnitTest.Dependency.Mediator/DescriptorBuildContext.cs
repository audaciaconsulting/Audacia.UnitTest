using Microsoft.Extensions.DependencyInjection;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// Context for building a descriptor in the mediator test framework.
/// </summary>
internal sealed record DescriptorBuildContext(
    TestTargetBuilder Builder,
    MediatorTestConfiguration Configuration,
    List<(Type Request, Type Response)> Handlers,
    IServiceCollection Services,
    HashSet<Type> Supplied);

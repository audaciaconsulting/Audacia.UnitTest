using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Audacia.UnitTest.Dependency.DependencyInjection;

/// <summary>
/// Supplies the <see cref="TestTargetBuilder"/> with the services a project registers in its own dependency
/// injection container, for example through an <c>AddApplication</c> extension method.
/// </summary>
/// <remarks>
/// <para>
/// Derive from this class in the test project, with a parameterless constructor, and it is discovered and used by
/// every builder in the same way as a blueprint. Alternatively, use
/// <see cref="TestTargetBuilderServicesExtensions.WithServices"/> for a single test.
/// </para>
/// <para>
/// Anything a registered service depends on that was not itself registered is built by the builder instead, using
/// its usual rules (instances given to <c>With</c>, blueprints, real implementations and so on). The container is
/// created the first time it is needed, so <c>With</c> and <c>WithBlueprint</c> can be called at any point before then.
/// Services come from one shared scope, so scoped services behave as they do within a single request. That scope
/// is not disposed, so avoid registering services that hold unmanaged resources.
/// </para>
/// </remarks>
public abstract class ServiceRegistration : IDependencySource
{
    private readonly ConditionalWeakTable<TestTargetBuilder, Container> _containers = [];

    /// <inheritdoc />
    public bool TryResolve(Type type, TestTargetBuilder builder, out object? dependency)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(builder);

        var container = _containers.GetValue(builder, CreateContainer);
        dependency = container.IsService(type) ? container.Scope.ServiceProvider.GetRequiredService(type) : null;

        return dependency is not null;
    }

    /// <summary>
    /// Registers the services, typically by calling the project's own <c>AddX</c> extension methods.
    /// </summary>
    /// <param name="services">The collection to register services with.</param>
    protected abstract void Register(IServiceCollection services);

    private Container CreateContainer(TestTargetBuilder builder)
    {
        var services = new ServiceCollection();
        Register(services);
        var delegatedToBuilder = AddUnregisteredDependencies(services, builder);

        return new Container(services, delegatedToBuilder);
    }

    /// <summary>
    /// Registers a stand-in for each dependency the registration did not provide, so the container can construct
    /// its services. Each stand-in asks the builder for the real dependency.
    /// </summary>
    /// <returns>The types the stand-ins were added for.</returns>
    private static HashSet<Type> AddUnregisteredDependencies(IServiceCollection services, TestTargetBuilder builder)
    {
        var registered = services.Select(descriptor => descriptor.ServiceType).ToHashSet();

        var missing = services
            .Select(descriptor => descriptor.ImplementationType)
            .OfType<Type>()
            .Where(type => !type.IsGenericTypeDefinition)
            .Distinct()
            .SelectMany(type => type.GetConstructors().FirstOrDefault()?.GetParameters() ?? [])
            .Where(parameter => !parameter.HasDefaultValue)
            .Select(parameter => parameter.ParameterType)
            .Where(type => NeedsBuilder(type) && !registered.Contains(type))
            .Distinct()
            .ToList();

        foreach (var type in missing)
        {
            services.AddSingleton(type, _ => builder.Build(type));
        }

        return [.. missing];
    }

    private static bool NeedsBuilder(Type type)
    {
        return !type.ContainsGenericParameters && type != typeof(IServiceProvider) && !IsEnumerable(type);
    }

    private static bool IsEnumerable(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }

    /// <summary>
    /// The built container, with the scope every service is resolved from. Nothing disposes it while the test runs:
    /// see the remarks on the class.
    /// </summary>
    private sealed class Container : IDisposable
    {
        private readonly IServiceProvider _rootProvider;

        private readonly IServiceProviderIsService _serviceChecker;

        private readonly HashSet<Type> _delegatedToBuilder;

        public Container(IServiceCollection services, HashSet<Type> delegatedToBuilder)
        {
            _delegatedToBuilder = delegatedToBuilder;
            _rootProvider = services.BuildServiceProvider();
            Scope = _rootProvider.CreateScope();
            _serviceChecker = Scope.ServiceProvider.GetRequiredService<IServiceProviderIsService>();
        }

        public IServiceScope Scope { get; }

        /// <summary>
        /// Whether the registration itself provides the <paramref name="type"/>. The stand-ins for dependencies it does
        /// not provide are excluded: they ask the builder, which would ask this container, and so on without end.
        /// </summary>
        public bool IsService(Type type)
        {
            return !_delegatedToBuilder.Contains(type) && _serviceChecker.IsService(type);
        }

        public void Dispose()
        {
            Scope.Dispose();
            (_rootProvider as IDisposable)?.Dispose();
        }
    }
}

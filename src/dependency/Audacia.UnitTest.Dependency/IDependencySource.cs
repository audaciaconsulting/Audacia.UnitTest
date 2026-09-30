namespace Audacia.UnitTest.Dependency;

/// <summary>
/// A source of dependencies that the <see cref="TestTargetBuilder"/> consults after any blueprint, and before it tries
/// to construct a dependency itself. This is how a project's own dependency injection registration can supply
/// the services that cannot be found by scanning, such as an interface whose implementation is only available through a container.
/// </summary>
/// <remarks>
/// <para>
/// Implementations with a parameterless constructor that are found in the blueprint assemblies are used
/// automatically, in the same way as blueprints. A new instance is created for each <see cref="TestTargetBuilder"/>.
/// </para>
/// </remarks>
public interface IDependencySource
{
    /// <summary>
    /// Attempts to supply an instance of the <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of the dependency required.</param>
    /// <param name="builder">The builder that needs the dependency, which the source can use to build anything else it needs.</param>
    /// <param name="dependency">The dependency, when this source can supply one.</param>
    /// <returns><see langword="true"/> if this source supplied the dependency, otherwise <see langword="false"/>.</returns>
    bool TryResolve(Type type, TestTargetBuilder builder, out object? dependency);
}

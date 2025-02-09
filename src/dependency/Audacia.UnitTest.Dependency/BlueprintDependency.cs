namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Blueprint for how to create a dependency of <typeparamref name="TDependency"/>.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency which is to be created by the blueprint.</typeparam>
public abstract class BlueprintDependency<TDependency> : IBlueprintDependency<TDependency>
    where TDependency : class
{
    /// <inheritdoc />
    public abstract TDependency Build();
}
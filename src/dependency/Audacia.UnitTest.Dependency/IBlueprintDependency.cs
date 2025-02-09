namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Blueprint for how to create a dependency of <typeparamref name="TDependency"/>.
/// </summary>
/// <typeparam name="TDependency">The type of object we're seeding.</typeparam>
public interface IBlueprintDependency<out TDependency>
    where TDependency : class
{
    /// <summary>
    /// Build a <typeparamref name="TDependency"/> based on the provided customisations, so we can seed it.
    /// </summary>
    /// <returns>The entity as specified in the seed configuration.</returns>
    TDependency Build();
}
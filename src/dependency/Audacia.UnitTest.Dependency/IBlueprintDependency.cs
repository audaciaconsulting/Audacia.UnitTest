namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Configuration for a single entity to be seeded into the database.
/// <br/>
/// This should seed exactly what is needed, with implementations differing on a per use case basis.
/// </summary>
/// <typeparam name="TDependency">The type of object we're seeding.</typeparam>
public interface IBlueprintDependency<out TDependency>
    where TDependency : class
{
    /// <summary>
    /// Gets an instance of the dependency.
    /// </summary>
    public TDependency Dependency { get; }

    /// <summary>
    /// Build a <typeparamref name="TDependency"/> based on the provided customisations, so we can seed it.
    /// </summary>
    /// <returns>The entity as specified in the seed configuration.</returns>
    TDependency Build();
}
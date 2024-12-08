using Audacia.UnitTest.Dependency.Customisations;
using Moq;

namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Configuration for a single entity to be seeded into the database.
/// <br/>
/// This should seed exactly what is needed, with implementations differing on a per use case basis.
/// </summary>
/// <typeparam name="TDependency">The type of object we're seeding.</typeparam>
public interface IBlueprintDependency<TDependency> where TDependency : class
{
    public Mock<TDependency> MockDependency { get; }

    /// <summary>
    /// Build a <typeparamref name="TDependency"/> based on the provided customisations, so we can seed it.
    /// </summary>
    /// <returns>The entity as specified in the seed configuration.</returns>
    TDependency Build();

    /// <summary>
    /// Allows for customisation of the dependency.
    /// </summary>
    ICollection<IBlueprintCustomisation<TDependency>> Customisations { get; }
}
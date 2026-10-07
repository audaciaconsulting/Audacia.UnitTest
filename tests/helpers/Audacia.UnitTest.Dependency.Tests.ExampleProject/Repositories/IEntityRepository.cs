namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Repositories;

/// <summary>
/// A repository for entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntityRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Gets the type of entity this repository was closed over.
    /// </summary>
    /// <returns>The entity type.</returns>
    Type GetEntityType();
}

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Repositories;

/// <summary>
/// An open generic implementation of <see cref="IEntityRepository{TEntity}"/>, which the builder must
/// close using the generic arguments of the interface being resolved.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EntityRepository<TEntity> : IEntityRepository<TEntity>
    where TEntity : class
{
    /// <inheritdoc />
    public Type GetEntityType()
    {
        return typeof(TEntity);
    }
}

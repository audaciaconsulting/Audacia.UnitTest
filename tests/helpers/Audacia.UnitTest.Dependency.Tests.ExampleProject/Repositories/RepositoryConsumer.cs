using Audacia.UnitTest.Dependency.Tests.ExampleProject.Configuration;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Repositories;

/// <summary>
/// Depends on a closed generic interface, so the builder has to resolve and close the open
/// generic implementation behind it.
/// </summary>
/// <remarks>
/// <para>Initializes a new instance of the <see cref="RepositoryConsumer"/> class.</para>
/// </remarks>
/// <param name="repository">The repository for the configuration entity.</param>
public sealed class RepositoryConsumer(IEntityRepository<AppConfiguration> repository)
{
    /// <summary>
    /// Gets the repository that was resolved.
    /// </summary>
    public IEntityRepository<AppConfiguration> Repository { get; } = repository;
}

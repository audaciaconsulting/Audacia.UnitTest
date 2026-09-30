namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Services;

/// <summary>
/// A service that only the container can supply: its implementation is internal, so it cannot be found by scanning.
/// </summary>
public interface IRegisteredOnlyService
{
    /// <summary>
    /// Gets a description of the dependency the service was built with.
    /// </summary>
    /// <returns>The description.</returns>
    string Describe();
}

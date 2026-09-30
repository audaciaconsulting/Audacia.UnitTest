namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Services;

/// <summary>
/// A dependency of <see cref="RegisteredOnlyService"/> that the registration does not add, so the builder must supply it.
/// </summary>
public interface IUnregisteredPart
{
    /// <summary>
    /// Gets the name of the part.
    /// </summary>
    string Name { get; }
}

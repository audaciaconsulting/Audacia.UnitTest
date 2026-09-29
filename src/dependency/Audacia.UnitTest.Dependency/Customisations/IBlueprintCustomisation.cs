namespace Audacia.UnitTest.Dependency.Customisations;

/// <summary>
/// Interface for customising a dependency to override the default behaviour.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency to be customised.</typeparam>
public interface IBlueprintCustomisation<TDependency> where TDependency : class
{
    /// <summary>
    /// Apply the customisation to the substitute for the dependency.
    /// </summary>
    /// <param name="substitute">Substitute to apply the customisation to.</param>
    void Apply(TDependency substitute);
}

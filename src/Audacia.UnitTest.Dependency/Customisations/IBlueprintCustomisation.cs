namespace Audacia.UnitTest.Dependency.Customisations;

/// <summary>
/// Interface for customising an entity to override default behaviour.
/// </summary>
/// <typeparam name="TDependency">The type of entity that the property belongs to.</typeparam>
public interface IBlueprintCustomisation<TDependency> where TDependency : class
{
    /// <summary>
    /// Apply the customisation on the dependency blueprint.
    /// </summary>
    /// <param name="blueprint">Blueprint to add the customisation too.</param>
    public void Apply(IBlueprintDependency<TDependency> blueprint);

    /// <summary>
    /// Ensure the setup of the customisation is valid.
    /// </summary>
    /// <param name="blueprintDependency">The dependency blueprint to validate against.</param>
    void Validate(BlueprintDependency<TDependency> blueprintDependency)
    {
    }
}
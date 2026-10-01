using NSubstitute;

namespace Audacia.UnitTest.Dependency.Blueprints;

/// <summary>
/// Blueprint for how to create a dependency of <typeparamref name="TDependency"/>.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency which is to be created by the blueprint.</typeparam>
public class BlueprintDependency<TDependency> : IBlueprintDependency<TDependency>
    where TDependency : class
{
    /// <summary>
    /// Gets or sets the mock instance of the dependency.
    /// </summary>
    public TDependency MockDependency { get; set; } = Substitute.For<TDependency>();

    /// <summary>
    /// Gets customisations to apply to mock instance of dependency.
    /// </summary>
    public ICollection<IBlueprintCustomisation<TDependency>> Customisations { get; } = [];

    /// <inheritdoc />
    public virtual TDependency Build()
    {
        foreach (var customisation in Customisations)
        {
            customisation.Apply(MockDependency);
        }

        return MockDependency;
    }
}
using Audacia.UnitTest.Dependency.Customisations;
using NSubstitute;

namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Blueprint for how to create a dependency of <typeparamref name="TDependency"/> with customisations applied to the mock instance.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency which is to be created by the blueprint.</typeparam>
public class CustomisedBlueprintDependency<TDependency> : BlueprintDependency<TDependency>
    where TDependency : class
{
    /// <summary>
    /// Gets or sets the mock instance of the dependency.
    /// </summary>
    public TDependency MockDependency { get; protected set; } = null!;

    /// <summary>
    /// Gets customisations to apply to mock instance of dependency.
    /// </summary>
    public ICollection<IBlueprintCustomisation<TDependency>> Customisations { get; } = [];

    /// <inheritdoc />
    public override TDependency Build()
    {
        if (!Customisations.Any())
        {
            return Substitute.For<TDependency>();
        }

        MockDependency = Substitute.For<TDependency>();

        foreach (var customisation in Customisations)
        {
            customisation.Apply(MockDependency);
        }

        return MockDependency;
    }
}

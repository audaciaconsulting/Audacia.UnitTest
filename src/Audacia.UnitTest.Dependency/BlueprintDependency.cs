using Audacia.UnitTest.Dependency.Customisations;
using Moq;

namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Customisation for dependency <typeparamref name="TDependency"/>.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency which is being customised.</typeparam>
public class BlueprintDependency<TDependency> : IBlueprintDependency<TDependency>
    where TDependency : class
{
    /// <summary>
    /// Gets or sets the mock instance of the dependency.
    /// </summary>
    public Mock<TDependency>? MockDependency { get; protected set; }

    /// <inheritdoc />
    public ICollection<IBlueprintCustomisation<TDependency>> Customisations { get; } = [];
    
    /// <inheritdoc />
    public virtual TDependency Build()
    {
        if (!Customisations.Any())
        {
            return Mock.Of<TDependency>();
        }

        MockDependency = new Mock<TDependency>();

        foreach (var customisation in Customisations)
        {
            customisation.Apply(this);
        }

        return MockDependency.Object;
    }
}
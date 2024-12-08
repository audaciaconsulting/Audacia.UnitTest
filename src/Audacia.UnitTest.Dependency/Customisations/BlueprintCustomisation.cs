using System.Linq.Expressions;

namespace Audacia.UnitTest.Dependency.Customisations;

/// <summary>
/// Customisation for a blueprint.
/// </summary>
/// <param name="setupExpression"></param>
/// <param name="result"></param>
/// <typeparam name="TDependency"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class BlueprintCustomisation<TDependency, TResult>(
    Expression<Func<TDependency, TResult>> setupExpression,
    TResult result)
    : IBlueprintCustomisation<TDependency> where TDependency : class
{
    /// <summary>
    /// Gets the expression of which of the mock instance to target for the customisation.
    /// </summary>
    private Expression<Func<TDependency, TResult>> MethodLambda { get; } = setupExpression;

    /// <summary>
    /// Gets the object which is configured to be returned as the result.
    /// </summary>
    private TResult Result { get; } = result;

    /// <summary>
    /// Applies the customisation to dependency blueprint.
    /// </summary>
    /// <param name="blueprint">The blueprint to apply the customisation too.</param>
    public void Apply(IBlueprintDependency<TDependency> blueprint)
    {
        ArgumentNullException.ThrowIfNull(blueprint);

        blueprint.MockDependency.Setup(MethodLambda).Returns(Result);
    }
}
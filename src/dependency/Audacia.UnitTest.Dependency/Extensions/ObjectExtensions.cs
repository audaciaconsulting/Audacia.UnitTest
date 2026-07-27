using Audacia.UnitTest.Dependency.Exceptions;

namespace Audacia.UnitTest.Dependency.Extensions;

/// <summary>
/// Extensions used by the <see cref="TestTargetBuilder"/> to build dependencies from their blueprints.
/// </summary>
internal static class ObjectExtensions
{
    /// <summary>
    /// Builds the dependency from an instance of its blueprint, by invoking
    /// <see cref="IBlueprintDependency{TDependency}.Build"/>.
    /// </summary>
    /// <param name="blueprintDependency">An instance of the blueprint.</param>
    /// <returns>The built dependency.</returns>
    /// <exception cref="BlueprintDependencyException">
    /// If the instance is not a blueprint, or its build method cannot be found.
    /// </exception>
    internal static object BuildDependencyFromBlueprint(this object blueprintDependency)
    {
        var blueprintType = blueprintDependency.GetType();

        var blueprintInterfaceType = blueprintType
                                         .GetInterfaces()
                                         .FirstOrDefault(
                                             interfaceType => interfaceType.IsGenericType &&
                                                              interfaceType.GetGenericTypeDefinition() ==
                                                              typeof(IBlueprintDependency<>))
                                     ?? throw new BlueprintDependencyException(
                                         $"Type {blueprintType.Name} does not implement {typeof(IBlueprintDependency<>).Name}.");

        var buildMethod = typeof(IBlueprintDependency<>)
            .MakeGenericType(blueprintInterfaceType.GetGenericArguments().First())
            .GetMethod(nameof(IBlueprintDependency<object>.Build));

        return buildMethod?.Invoke(blueprintDependency, null)
               ?? throw new BlueprintDependencyException(
                   "Unable to find 'Build' to allow for building dependency blueprint.");
    }
}

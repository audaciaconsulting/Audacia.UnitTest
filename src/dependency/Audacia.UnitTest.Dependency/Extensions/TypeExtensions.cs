using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Audacia.UnitTest.Dependency.Extensions;

/// <summary>
/// Extensions used by the <see cref="TestTargetBuilder"/> to inspect types being resolved.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Gets the start of the project name for the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to resolve the project name from.</param>
    /// <returns>The start of the project name, for example 'Audacia' from 'Audacia.Commands'.</returns>
    internal static string GetProjectStartName(this Type type)
    {
        return type.Assembly.FullName!.Split('.').First();
    }

    /// <summary>
    /// Gets the type implementing the interface <paramref name="type"/> from <paramref name="allTypes"/>.
    /// </summary>
    /// <param name="type">The interface type to find an implementation for.</param>
    /// <param name="allTypes">The types to search for an implementation.</param>
    /// <returns>
    /// The first type implementing the interface, or an open generic implementation where the
    /// interface is generic. Returns <see langword="null"/> when no implementation is found.
    /// </returns>
    internal static Type? GetInterfaceImplementationType(this Type type, IReadOnlyCollection<Type> allTypes)
    {
        var firstImplementationType = allTypes.FirstOrDefault(type.IsAssignableFrom);
        if (firstImplementationType != null)
        {
            return firstImplementationType;
        }

        if (!type.IsGenericType)
        {
            return null;
        }

        var implementationTypes = allTypes
            .Where(candidate => candidate is { IsAbstract: false, IsInterface: false })
            .Where(
                candidate => candidate.GetInterfaces().Any(
                    interfaceType => interfaceType.IsGenericType &&
                                     interfaceType.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()));

        return implementationTypes.FirstOrDefault(
            implementationType => implementationType is { IsGenericType: true, ContainsGenericParameters: true });
    }

    /// <summary>
    /// Checks whether the given <paramref name="type"/> is an <see cref="ILogger{TCategoryName}"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is an <see cref="ILogger{TCategoryName}"/>.</returns>
    internal static bool IsLogger(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ILogger<>);
    }

    /// <summary>
    /// Checks whether the given <paramref name="type"/> is an <see cref="IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is an <see cref="IOptions{TOptions}"/>.</returns>
    internal static bool IsOptions(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IOptions<>);
    }

    /// <summary>
    /// Gets the <see cref="OptionsWrapper{TOptions}"/> type for the given options <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The options type.</param>
    /// <returns>The closed <see cref="OptionsWrapper{TOptions}"/> type.</returns>
    internal static Type GetOptionsWrapper(this Type type)
    {
        var typeForOptions = type.GetGenericArguments().First();

        return typeof(OptionsWrapper<>).MakeGenericType(typeForOptions);
    }

    /// <summary>
    /// Checks whether the given <paramref name="type"/> implements <see cref="IBlueprintDependency{TDependency}"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type implements <see cref="IBlueprintDependency{TDependency}"/>.</returns>
    internal static bool IsBlueprintDependency(this Type type)
    {
        return type.GetInterfaces().Any(
            interfaceType => interfaceType.IsGenericType &&
                             interfaceType.GetGenericTypeDefinition() == typeof(IBlueprintDependency<>));
    }

    /// <summary>
    /// Gets all concrete classes implementing <see cref="IBlueprintDependency{TDependency}"/>.
    /// </summary>
    /// <param name="types">The types to search.</param>
    /// <returns>The types that are usable blueprints.</returns>
    internal static IEnumerable<Type> GetBlueprintDependencies(this IEnumerable<Type> types)
    {
        return types.Where(type => type is { IsClass: true, IsAbstract: false } && type.IsBlueprintDependency());
    }

    /// <summary>
    /// Determines whether <paramref name="blueprintType"/> is a blueprint for <paramref name="dependencyType"/>.
    /// </summary>
    /// <param name="blueprintType">The candidate blueprint type.</param>
    /// <param name="dependencyType">The dependency the blueprint should build.</param>
    /// <returns><see langword="true"/> if the blueprint builds the dependency.</returns>
    internal static bool IsBlueprintFor(this Type blueprintType, Type dependencyType)
    {
        var blueprintInterfaceType = blueprintType.GetInterface(typeof(IBlueprintDependency<>).Name);
        if (blueprintInterfaceType is null)
        {
            return false;
        }

        var blueprintGenericArgument = blueprintInterfaceType.GetGenericArguments().First();

        return blueprintGenericArgument == dependencyType ||
               (blueprintGenericArgument.IsGenericType &&
                dependencyType.IsGenericType &&
                blueprintGenericArgument.GetGenericTypeDefinition() == dependencyType.GetGenericTypeDefinition());
    }
}

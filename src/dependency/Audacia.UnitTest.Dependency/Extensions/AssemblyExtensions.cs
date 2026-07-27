using System.Reflection;
using Audacia.UnitTest.Dependency.Attributes;
using Audacia.UnitTest.Dependency.Exceptions;

namespace Audacia.UnitTest.Dependency.Extensions;

/// <summary>
/// Extensions used by the <see cref="TestTargetBuilder"/> to discover the assemblies dependencies can be resolved from.
/// </summary>
internal static class AssemblyExtensions
{
    /// <summary>
    /// Gets the <paramref name="executingAssembly"/> along with every assembly it references, directly or
    /// transitively, that is within the <paramref name="projectNamespace"/> and not excluded.
    /// </summary>
    /// <param name="executingAssembly">The assembly to start from.</param>
    /// <param name="projectNamespace">The project namespace assemblies must start with.</param>
    /// <param name="excludeNamespaces">Namespaces to exclude from the search.</param>
    /// <returns>The executing assembly and all matching referenced assemblies.</returns>
    internal static IReadOnlyCollection<Assembly> GetWithAllReferencedAssemblies(
        this Assembly executingAssembly,
        string projectNamespace,
        IReadOnlyCollection<string> excludeNamespaces)
    {
        var referencedAssemblies = executingAssembly.GetAllReferencedAssemblies(projectNamespace, excludeNamespaces);

        return [executingAssembly, .. referencedAssemblies];
    }

    /// <summary>
    /// Walks the whole reference graph from <paramref name="executingAssembly"/>, so assemblies referenced
    /// indirectly are found as well as direct references.
    /// </summary>
    /// <param name="executingAssembly">The assembly to start from.</param>
    /// <param name="projectNamespace">The project namespace assemblies must start with.</param>
    /// <param name="excludeNamespaces">Namespaces to exclude from the search.</param>
    /// <returns>All matching referenced assemblies.</returns>
    internal static IReadOnlyCollection<Assembly> GetAllReferencedAssemblies(
        this Assembly executingAssembly,
        string projectNamespace,
        IReadOnlyCollection<string> excludeNamespaces)
    {
        var processedAssemblies = new HashSet<string> { executingAssembly.FullName! };
        var assembliesToProcess = new Queue<Assembly>();
        var referencedAssemblies = new List<Assembly>();

        assembliesToProcess.Enqueue(executingAssembly);

        while (assembliesToProcess.Count > 0)
        {
            var newAssemblies = assembliesToProcess.Dequeue()
                .LoadUnprocessedReferences(projectNamespace, excludeNamespaces, processedAssemblies);

            referencedAssemblies.AddRange(newAssemblies);
            newAssemblies.ForEach(assembliesToProcess.Enqueue);
        }

        return referencedAssemblies;
    }

    /// <summary>
    /// Loads the references of <paramref name="assembly"/> that are in scope and have not already been seen.
    /// </summary>
    private static List<Assembly> LoadUnprocessedReferences(
        this Assembly assembly,
        string projectNamespace,
        IReadOnlyCollection<string> excludeNamespaces,
        HashSet<string> processedAssemblies)
    {
        return
        [
            .. assembly.GetReferencedAssemblies()
                .Where(assemblyName => assemblyName.FullName.StartsWith(projectNamespace, StringComparison.Ordinal))
                .Where(assemblyName => !assemblyName.IsExcluded(excludeNamespaces))
                .Where(assemblyName => processedAssemblies.Add(assemblyName.FullName))
                .Select(Assembly.Load)
        ];
    }

    /// <summary>
    /// Gets the public classes visible outside of the given <paramref name="assemblies"/>.
    /// </summary>
    /// <param name="assemblies">The assemblies to get types from.</param>
    /// <returns>The exported classes.</returns>
    internal static IReadOnlyCollection<Type> GetAllExportedClasses(this IEnumerable<Assembly> assemblies)
    {
        return
        [
            .. assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => type.IsClass)
        ];
    }

    /// <summary>
    /// Loads every assembly marked with the <see cref="BlueprintAssemblyAttribute"/>, falling back to the
    /// <paramref name="executingAssembly"/> when none are marked.
    /// </summary>
    /// <param name="executingAssembly">The assembly to read the attributes from.</param>
    /// <returns>The assemblies blueprints should be discovered in.</returns>
    /// <exception cref="BlueprintDependencyException">If a marked assembly cannot be loaded.</exception>
    internal static IReadOnlyCollection<Assembly> GetAllBlueprintAssemblies(this Assembly executingAssembly)
    {
        var blueprintAssemblies = executingAssembly.GetCustomAttributes<BlueprintAssemblyAttribute>()
            .Select(
                blueprintAssemblyAttribute => Assembly.Load(blueprintAssemblyAttribute.Name)
                                              ?? throw new BlueprintDependencyException(
                                                  $"Unable to load assembly {blueprintAssemblyAttribute.Name}. Ensure it is referenced in the project."))
            .ToList();

        return blueprintAssemblies.Count == 0 ? [executingAssembly] : blueprintAssemblies;
    }

    /// <summary>
    /// Gets the blueprint type that builds the given <paramref name="dependencyType"/>, if one exists.
    /// </summary>
    /// <param name="assemblies">The assemblies to search.</param>
    /// <param name="dependencyType">The dependency a blueprint is needed for.</param>
    /// <returns>The blueprint type, or <see langword="null"/> when none is found.</returns>
    internal static Type? GetBlueprintDependencyType(this IEnumerable<Assembly> assemblies, Type dependencyType)
    {
        return assemblies
            .SelectMany(assembly => assembly.GetExportedTypes())
            .GetBlueprintDependencies()
            .FirstOrDefault(blueprintType => blueprintType.IsBlueprintFor(dependencyType));
    }

    private static bool IsExcluded(this AssemblyName assemblyName, IReadOnlyCollection<string> excludeNamespaces)
    {
        return excludeNamespaces.Any(
            exclude => assemblyName.FullName.Contains(exclude, StringComparison.CurrentCultureIgnoreCase));
    }
}

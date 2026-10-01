using System.Collections.Concurrent;
using Audacia.UnitTest.Dependency.Exceptions;
using Audacia.UnitTest.Dependency.Extensions;
using Audacia.UnitTest.Dependency.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Audacia.UnitTest.Dependency;

/// <summary>
/// A builder for constructing a target for ease of use dependency building.
/// </summary>
public class TestTargetBuilder
{
    /// <summary>
    /// The full name of Moq's mock type, matched by name so this package does not depend on Moq. This is used to guard against passing a mock wrapper rather than an instance.
    /// </summary>
    private const string MoqMockTypeName = "Moq.Mock`1";

    private readonly string[] _excludeNamespaces;

    private readonly Dictionary<Type, object> _services = [];

    private readonly Dictionary<Type, object> _blueprints = [];

    private readonly List<DependencyResolution> _resolutions = [];

    /// <summary>
    /// All types that can be resolved as a dependency, keyed by their consuming assembly, project scope and exclusions.
    /// Made a static field as part of #167094, as we were getting errors dynamically loading assemblies when this was a local variable.
    /// </summary>
    private static readonly ConcurrentDictionary<string, IReadOnlyCollection<Type>> TypesForAssembly = new();

    /// <summary>
    /// The exported types of the blueprint assemblies, keyed by the consuming assembly, so they are only scanned once
    /// rather than each time a dependency is resolved.
    /// </summary>
    private static readonly ConcurrentDictionary<string, IReadOnlyCollection<Type>> BlueprintAssemblyTypesForAssembly = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTargetBuilder"/> class.
    /// </summary>
    public TestTargetBuilder()
    {
        _excludeNamespaces = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTargetBuilder"/> class along with configuring to excluding
    /// certain namespaces when trying to find blue dependencies.
    /// </summary>
    /// <param name="excludeNamespaces">
    /// A collection of namespaces which are excluded when finding blueprint dependencies.
    /// </param>
    public TestTargetBuilder(IEnumerable<string> excludeNamespaces)
    {
        ArgumentNullException.ThrowIfNull(excludeNamespaces);

        _excludeNamespaces = [.. excludeNamespaces];
    }

    /// <summary>
    /// Configures the Test Target Builder with an instance of one of it's dependencies.
    /// </summary>
    /// <param name="service">An instance of a dependency.</param>
    /// <typeparam name="TDependency">The type of the dependency.</typeparam>
    /// <returns>The Test Target builder.</returns>
    public TestTargetBuilder With<TDependency>(TDependency service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var dependencyType = typeof(TDependency);
        return WithDependency(service, dependencyType);
    }

    /// <summary>
    /// Configures the Test Target Builder with an blueprint of one of it's dependencies.
    /// </summary>
    /// <param name="blueprint">An instance of a blueprint for a dependency.</param>
    /// <typeparam name="TDependency">The type of the dependency.</typeparam>
    /// <returns>The Test Target builder.</returns>
    /// <exception cref="TestTargetBuilderException">If multiple blueprints for the same type have being configured.</exception>
    public TestTargetBuilder WithBlueprint<TDependency>(IBlueprintDependency<TDependency> blueprint)
        where TDependency : class
    {
        ArgumentNullException.ThrowIfNull(blueprint);

        var type = typeof(TDependency);

        return !_blueprints.TryAdd(type, blueprint)
            ? throw new TestTargetBuilderException("This blueprint has already been added", nameof(blueprint))
            : this;
    }

    /// <summary>
    /// Injects <see cref="IOptions{TOptions}"/> into the builder. When the test target or its dependencies accesses the value
    /// they will use the current value of the <paramref name="options"></paramref>,
    /// so you can modify the value whenever you need.
    /// </summary>
    /// <param name="options">The value of the options to be provided to the test target.</param>
    /// <typeparam name="TOptions">The type of the options class which is being provided to the test target.</typeparam>
    /// <returns>The TestTargetBuilder.</returns>
    public TestTargetBuilder WithOptions<TOptions>(TOptions options)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(options);

        var optionsWrapper = new OptionsWrapper<TOptions>(options);
        return With<IOptions<TOptions>>(optionsWrapper);
    }

    /// <summary>
    /// Injects <see cref="IOptionsSnapshot{TOptions}"/> into the builder with named options support. When the test
    /// target or its dependencies call <c>Get(name)</c> they receive the corresponding named configuration.
    /// </summary>
    /// <param name="namedOptions">The configurations to provide, keyed by name.</param>
    /// <param name="defaultOptions">
    /// The configuration used for the <see cref="IOptions{TOptions}.Value"/> property, and for any name that was not
    /// configured. When <see langword="null"/> the first entry in <paramref name="namedOptions"/> is used.
    /// </param>
    /// <typeparam name="TOptions">The type of the options class which is being provided to the test target.</typeparam>
    /// <returns>The TestTargetBuilder.</returns>
    /// <exception cref="ArgumentException">If <paramref name="namedOptions"/> is empty.</exception>
    public TestTargetBuilder WithOptionsSnapshot<TOptions>(
        IReadOnlyDictionary<string, TOptions> namedOptions,
        TOptions? defaultOptions = null)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(namedOptions);

        if (namedOptions.Count == 0)
        {
            throw new ArgumentException("At least one named option must be provided", nameof(namedOptions));
        }

        var defaultValue = defaultOptions ?? namedOptions.First().Value;
        var snapshot = new NamedOptionsSnapshot<TOptions>(namedOptions, defaultValue);

        return With<IOptionsSnapshot<TOptions>>(snapshot);
    }

    /// <summary>
    /// Gets the dependencies the builder has chosen automatically so far, in the order they were completed, so a test
    /// can see what it used. Dependencies supplied explicitly, for example with <see cref="With{TDependency}"/> or
    /// <see cref="WithBlueprint{TDependency}"/>, are not included.
    /// </summary>
    public IReadOnlyList<DependencyResolution> Resolutions => _resolutions;

    /// <summary>
    /// Describes the dependencies the builder has chosen automatically so far, one per line, giving the full name of the
    /// concrete type used and how it was chosen. Loggers and default options are summarised rather than listed.
    /// </summary>
    /// <returns>The description, or an empty string when nothing has been chosen automatically.</returns>
    public string DescribeResolutions()
    {
        return ResolutionDescriber.Describe(_resolutions);
    }

    /// <summary>
    /// Constructs an instance of the specified target type.
    /// </summary>
    /// <typeparam name="TTarget">Type to create instance of.</typeparam>
    /// <returns>An instance of <typeparamref name="TTarget"/>.</returns>
    public TTarget Build<TTarget>()
        where TTarget : class
    {
        return (TTarget)Build(typeof(TTarget));
    }

    /// <summary>
    /// Constructs an instance of the specified target type, where the type is only known at runtime.
    /// </summary>
    /// <param name="targetType">Type to create instance of.</param>
    /// <returns>An instance of <paramref name="targetType"/>.</returns>
    public virtual object Build(Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        return GetOrCreateService(targetType, new DependencyChain());
    }

    private TestTargetBuilder WithDependency(
        object service,
        Type type)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(type);

        GuardAgainstUnsupportedDependency(type);

        return !_services.TryAdd(type, service)
            ? throw new TestTargetBuilderException("This service has already been added", nameof(service))
            : this;
    }

    /// <summary>
    /// Rejects values that are a common mistake to pass, so the failure is explained rather than
    /// surfacing later as an unresolvable dependency.
    /// </summary>
    /// <exception cref="TestTargetBuilderException">If a mock wrapper or a blueprint was provided.</exception>
    private static void GuardAgainstUnsupportedDependency(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == MoqMockTypeName)
        {
            throw new TestTargetBuilderException(
                "A mock was provided rather than an instance. Use NSubstitute to create a mock instance instead.",
                type.Name);
        }

        if (type.IsBlueprintDependency())
        {
            throw new TestTargetBuilderException(
                $"A blueprint was provided to '{nameof(With)}'. Use '{nameof(WithBlueprint)}' to configure a dependency from its blueprint.",
                type.Name);
        }
    }

    private object GetOrCreateService(
        Type type,
        DependencyChain dependencyChain,
        bool recordResolution = true)
    {
        if (_services.TryGetValue(type, out var service))
        {
            return service;
        }

        var serviceInstance = ResolveService(type, dependencyChain, recordResolution);

        // Do not cache generic types, as the same definition resolves differently depending on its parent.
        if (!type.IsGenericType)
        {
            _services.Add(type, serviceInstance);
        }

        return serviceInstance;
    }

    private object ResolveService(
        Type type,
        DependencyChain dependencyChain,
        bool recordResolution)
    {
        try
        {
            dependencyChain.Push(type);

            var resolved = GetDependencyFromBlueprint(type) ??
                           Automatic(GetClassService(type, dependencyChain), ResolutionKind.Class) ??
                           Automatic(GetOptions(type, dependencyChain), ResolutionKind.Options) ??
                           Automatic(GetLoggerService(type), ResolutionKind.Logger) ??
                           Automatic(GetInterfaceService(type, dependencyChain), ResolutionKind.Interface) ??
                           throw new TestTargetBuilderException(GetErrorMessage(type, dependencyChain), type.Name);

            if (recordResolution)
            {
                Record(type, resolved);
            }

            return resolved.Instance;
        }
        catch (TestTargetBuilderException)
        {
            // No need to do anything - let the exception bubble up.
            // The point of this catch block is to prevent the exception triggering
            // the more general catch block below, as we are in a recursive method
            // and don't need an inner exception for every level of recursion.
            throw;
        }
        catch (Exception exception)
        {
            throw new TestTargetBuilderException(GetErrorMessage(type, dependencyChain), type.Name, exception);
        }
        finally
        {
            dependencyChain.Pop();
        }
    }

    private static ResolvedDependency? Automatic(object? instance, ResolutionKind kind)
    {
        return instance is null ? null : new ResolvedDependency(instance, kind, null, IsExplicit: false);
    }

    private void Record(Type requestedType, ResolvedDependency resolved)
    {
        if (resolved.IsExplicit)
        {
            return;
        }

        var resolution = new DependencyResolution(requestedType, resolved.Kind, resolved.Via, resolved.Instance.GetType());
        if (!_resolutions.Contains(resolution))
        {
            _resolutions.Add(resolution);
        }
    }

    /// <summary>
    /// Creates an instance of the <paramref name="dependencyType"/> from its blueprint, if one exists.
    /// </summary>
    private ResolvedDependency? GetDependencyFromBlueprint(Type dependencyType)
    {
        var blueprint = GetBlueprintForDependency(dependencyType);

        return blueprint is null
            ? null
            : new ResolvedDependency(
                blueprint.BuildDependencyFromBlueprint(),
                ResolutionKind.Blueprint,
                blueprint.GetType(),
                _blueprints.ContainsKey(dependencyType));
    }

    /// <summary>
    /// Creates an instance of the <paramref name="type"/> if it is a class, resolving its constructor
    /// parameters through the builder. Open generic classes are closed using the generic arguments of
    /// their parent, for example <c>GetCommand{TContext,TEntity}</c> from <c>IGetCommand{HubContext,Job}</c>.
    /// </summary>
    /// <exception cref="TestTargetBuilderException">If the type has no constructor.</exception>
    private object? GetClassService(
        Type type,
        DependencyChain dependencyChain)
    {
        if (!type.IsClass)
        {
            return null;
        }

        var typeToConstruct = type;
        if (type.IsGenericTypeDefinition)
        {
            var parentType = dependencyChain.GetParentType();
            typeToConstruct = type.MakeGenericType(parentType.GetGenericArguments());
        }

        var constructor = typeToConstruct.GetConstructors().FirstOrDefault()
                          ?? throw new TestTargetBuilderException(
                              $"Cannot find a Constructor for {type.Name}",
                              type.Name);

        var valuesForConstructor = constructor.GetParameters()
            .Select(parameterInfo => GetOrCreateService(parameterInfo.ParameterType, dependencyChain.Clone()))
            .ToArray();

        return constructor.Invoke(valuesForConstructor);
    }

    /// <summary>
    /// Creates an instance of the <paramref name="type"/> if it is an <see cref="IOptions{TOptions}"/>.
    /// </summary>
    private object? GetOptions(
        Type type,
        DependencyChain dependencyChain)
    {
        return type.IsOptions() ? GetOrCreateService(type.GetOptionsWrapper(), dependencyChain, recordResolution: false) : null;
    }

    /// <summary>
    /// Creates an instance of the <paramref name="type"/> if it is an <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/>.
    /// </summary>
    private static object? GetLoggerService(Type type)
    {
        if (!type.IsLogger())
        {
            return null;
        }

        var loggerType = typeof(NullLogger<>).MakeGenericType(type.GetGenericArguments().First());

        return Activator.CreateInstance(loggerType);
    }

    /// <summary>
    /// Creates an instance of the <paramref name="type"/> if it is an interface, by finding an implementation of it.
    /// </summary>
    private object? GetInterfaceService(
        Type type,
        DependencyChain dependencyChain)
    {
        if (!type.IsInterface)
        {
            return null;
        }

        var types = GetAllTypesInProjectScope(type);

        var implementationType = type.GetInterfaceImplementationType(types);

        return implementationType is null ? null : GetOrCreateService(implementationType, dependencyChain, recordResolution: false);
    }

    /// <summary>
    /// Finds the blueprint for the <paramref name="dependencyType"/>, if one exists. A generic blueprint is
    /// closed using the generic arguments of the dependency.
    /// </summary>
    private object? GetBlueprintForDependency(Type dependencyType)
    {
        // If the test target builder has been configured with the dependency type use that instead of the default blueprint.
        if (_blueprints.TryGetValue(dependencyType, out var existingBlueprintDependency))
        {
            return existingBlueprintDependency;
        }

        var blueprintDependencyType = GetBlueprintAssemblyTypes().GetBlueprintDependencyType(dependencyType);
        if (blueprintDependencyType is null)
        {
            return null;
        }

        if (blueprintDependencyType.IsGenericTypeDefinition)
        {
            blueprintDependencyType = CloseGenericBlueprint(blueprintDependencyType, dependencyType);
        }

        return Activator.CreateInstance(blueprintDependencyType);
    }

    /// <summary>
    /// Closes a generic blueprint using the generic arguments of the dependency it builds.
    /// </summary>
    /// <exception cref="TestTargetBuilderException">If the blueprint is generic but the dependency is not.</exception>
    private static Type CloseGenericBlueprint(
        Type blueprintDependencyType,
        Type dependencyType)
    {
        return !dependencyType.IsGenericType
            ? throw new TestTargetBuilderException(
                $"Blueprint type {blueprintDependencyType.Name} is generic but dependency type {dependencyType.Name} is not generic",
                dependencyType.Name)
            : blueprintDependencyType.MakeGenericType(dependencyType.GetGenericArguments());
    }

    /// <summary>
    /// Gets the exported types of the assemblies blueprints are discovered in.
    /// </summary>
    private static IReadOnlyCollection<Type> GetBlueprintAssemblyTypes()
    {
        var executingAssembly = EntryPointAssembly.Load();

        return BlueprintAssemblyTypesForAssembly.GetOrAdd(
            executingAssembly.GetName().Name!,
            _ => executingAssembly.GetAllBlueprintAssemblies().GetAllExportedTypes());
    }

    /// <summary>
    /// Gets all types from the assemblies in the same project scope as the <paramref name="typeToResolve"/>.
    /// </summary>
    private IReadOnlyCollection<Type> GetAllTypesInProjectScope(Type typeToResolve)
    {
        // e.g. get "Audacia" from "Audacia.Commands" so we only load "Audacia" assemblies.
        var startingProjectNamespace = typeToResolve.GetProjectStartName();
        var executingAssembly = EntryPointAssembly.Load();

        // The project scope and exclusions form part of the key, as each combination resolves a different set of types.
        var key = $"{executingAssembly.GetName().Name}|{startingProjectNamespace}|{string.Join(",", _excludeNamespaces)}";

        return TypesForAssembly.GetOrAdd(
            key,
            _ => executingAssembly
                .GetWithAllReferencedAssemblies(startingProjectNamespace, _excludeNamespaces)
                .GetAllExportedClasses());
    }

    /// <summary>
    /// Gets the error message for constructing the <paramref name="type"/> within the <paramref name="dependencyChain"/>.
    /// </summary>
    private string GetErrorMessage(
        Type type,
        DependencyChain dependencyChain)
    {
        var parentMessage = dependencyChain.Count != 0
            ? $" (constructing these types: {dependencyChain.ToChainString()})"
            : string.Empty;

        var resolved = DescribeResolutions();
        var resolvedMessage = resolved.Length == 0
            ? string.Empty
            : $"{Environment.NewLine}Dependencies chosen automatically so far (those supplied with With, WithBlueprint and so on are not listed):{Environment.NewLine}{resolved}";

        return $"Could not construct a service for {type.Name}{parentMessage}{resolvedMessage}";
    }

    /// <summary>
    /// A dependency the builder found, and how.
    /// </summary>
    /// <param name="Instance">The dependency.</param>
    /// <param name="Kind">How it was found.</param>
    /// <param name="Via">The blueprint that supplied it, if any.</param>
    /// <param name="IsExplicit">Whether the test asked for it, rather than the builder choosing it.</param>
    private sealed record ResolvedDependency(object Instance, ResolutionKind Kind, Type? Via, bool IsExplicit);
}

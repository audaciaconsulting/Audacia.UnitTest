namespace Audacia.UnitTest.Dependency;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Audacia.UnitTest.Dependency.Attributes;
using Audacia.UnitTest.Dependency.Exceptions;
using Audacia.UnitTest.Dependency.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

/// <summary>
/// A builder for constructing a target for ease of use dependency building.
/// </summary>
public class TestTargetBuilder
{
    private readonly IEnumerable<string> _excludeNamespaces = new List<string>();

    private readonly IDictionary<Type, object> _services = new Dictionary<Type, object>();

    private readonly IDictionary<Type, object> _blueprints = new Dictionary<Type, object>();

    /// <summary>
    /// All types that can be resolved as a dependency, keyed by their consuming assembly
    /// Made a static field as part of #167094, as we were getting errors dynamically loading assemblies when this was a local variable.
    /// </summary>
    private static readonly ConcurrentDictionary<string, IEnumerable<Type>> TypesForAssembly = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTargetBuilder"/> class.
    /// </summary>
    public TestTargetBuilder()
    {
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
        this._excludeNamespaces = excludeNamespaces;
    }

    private TestTargetBuilder WithDependency(
        object service,
        Type type)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(type);

        if (!this._services.TryAdd(type, service))
        {
            throw new TestTargetBuilderException("This service has already been added", nameof(service));
        }

        return this;
    }

    /// <summary>
    /// Configures the Test Target Builder with an instance of one of it's dependencies.
    /// </summary>
    /// <param name="service"></param>
    /// <typeparam name="TDependency"></typeparam>
    /// <returns></returns>
    public TestTargetBuilder With<TDependency>(TDependency service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var dependencyType = typeof(TDependency);
        return this.WithDependency(service, dependencyType);
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

        if (!this._blueprints.TryAdd(type, blueprint))
        {
            throw new TestTargetBuilderException("This blueprint has already been added", nameof(blueprint));
        }

        return this;
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
        return this.With<IOptions<TOptions>>(optionsWrapper);
    }

    /// <summary>
    /// Constructs an instance of the specified target type.
    /// </summary>
    /// <typeparam name="TTarget">Type to create instance of.</typeparam>
    /// <returns>An instance of <typeparamref name="TTarget"/>.</returns>
    public TTarget Build<TTarget>()
        where TTarget : class
    {
        return (TTarget)this.GetOrCreateService(typeof(TTarget), []);
    }

    private static string GetErrorMessage(
        Type type,
        ICollection<string> parentTypes)
    {
        var parentMessage = parentTypes.Count != 0
            ? $" (constructing these types: {string.Join(" > ", parentTypes)})"
            : string.Empty;

        return $"Could not construct a service for {type.Name}{parentMessage}";
    }

    private static IEnumerable<Type> GetAllTypes(
        string projectNamespace,
        params string[] excludes)
    {
        var executingAssembly = EntryPointAssembly.Load();
        var key = executingAssembly.GetName().Name!;
        if (!TypesForAssembly.TryGetValue(key, out var types))
        {
            var referencedAssemblies = executingAssembly.GetReferencedAssemblies()
                .Where(assemblyName => assemblyName.FullName.StartsWith(projectNamespace))
                .Where(
                    assemblyName => !excludes.Any() || excludes.Any(
                        exclude => !assemblyName.FullName.Contains(exclude, StringComparison.CurrentCultureIgnoreCase)))
                .Select(Assembly.Load)
                .ToList();
            List<Assembly> allAssemblies = [executingAssembly, .. referencedAssemblies];
            types = allAssemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => t.IsClass)
                .ToList();
            TypesForAssembly.TryAdd(key, types);
        }

        return types;
    }

    private static object? BuildDependencyFromBlueprint(object blueprintDependency)
    {
        var blueprintDependencyType = blueprintDependency.GetType();

        var buildBlueprintDependencyMethod = blueprintDependencyType.GetMethod("Build");

        if (buildBlueprintDependencyMethod is null)
        {
            throw new BlueprintDependencyException(
                "Unable to find 'Build' to allow for building dependency blueprint.");
        }

        return buildBlueprintDependencyMethod.Invoke(blueprintDependency, null);
    }


    private static void ValidateCustomiseExpression<TDependency, TResult>(
        Expression<Func<TDependency, TResult>> setupExpression)
        where TDependency : class where TResult : class
    {
        if (setupExpression.Body is not MethodCallExpression)
        {
            throw new ArgumentException("Setup expression must be a method call.", nameof(setupExpression));
        }
    }

    private object? GetInterfaceService(
        Type typeToResolve,
        ICollection<string> parentTypes)
    {
        if (!typeToResolve.IsInterface)
        {
            return null;
        }

        var assembly = typeToResolve.Assembly;
        var startingProjectNamespace = assembly.FullName!.Split('.').First();
        var types = GetAllTypes(startingProjectNamespace);

        var firstImplementationType = types.FirstOrDefault(typeToResolve.IsAssignableFrom);
        if (firstImplementationType == null)
        {
            var implementationTypes = types.Where(type => type is { IsAbstract: false, IsInterface: false }).Where(
                type => type.GetInterfaces().Any(
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeToResolve.GetGenericTypeDefinition()));

            var genericImplementationType = implementationTypes.FirstOrDefault(
                it => it is { IsGenericType: true, ContainsGenericParameters: true });

            if (genericImplementationType == null)
            {
                return null;
            }

            parentTypes.Add(typeToResolve.Name);

            return this.GetOrCreateService(genericImplementationType, parentTypes);
        }

        parentTypes.Add(typeToResolve.Name);

        return this.GetOrCreateService(firstImplementationType, parentTypes);
    }

    private object? GetClassService(
        Type type,
        ICollection<string> parentTypes)
    {
        if (!type.IsClass)
        {
            return null;
        }

        var constructor = type.GetConstructors().FirstOrDefault()
                          ?? throw new TestTargetBuilderException(
                              $"Cannot find a Constructor for {type.Name}",
                              nameof(type));
        var constructorParameters = constructor.GetParameters();

        parentTypes.Add(type.Name);

        var valuesForConstructor = constructorParameters
            .Select(parameterInfo => this.GetOrCreateService(parameterInfo.ParameterType, parentTypes))
            .ToArray();

        var serviceInstance = constructor.Invoke(valuesForConstructor);

        return serviceInstance;
    }

    private object GetOrCreateService(
        Type type,
        ICollection<string> parentTypes)
    {
        if (TryGetDependencyFromCache(type, out var service))
        {
            return service;
        }

        object serviceInstance;
        try
        {
            serviceInstance = GetBlueprintDependency(type, parentTypes) ??
                              GetClassService(type, parentTypes) ??
                              GetOptions(type, parentTypes) ??
                              GetLoggerService(type) ??
                              GetInterfaceService(type, parentTypes) ??
                              throw new BlueprintDependencyException("Unable to get dependency");
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
            var errorMessage = GetErrorMessage(type, parentTypes);
            throw new TestTargetBuilderException(errorMessage, nameof(type), exception);
        }

        if (serviceInstance == null)
        {
            var errorMessage = GetErrorMessage(type, parentTypes);
            throw new TestTargetBuilderException(errorMessage, nameof(type));
        }

        this._services.Add(type, serviceInstance);

        return serviceInstance;
    }

    private bool TryGetDependencyFromCache(
        Type type,
        out object? service)
    {
        var canGetService = this._services.TryGetValue(type, out service);

        return canGetService;
    }

    private object? GetLoggerService(Type type)
    {
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ILogger<>) ||
            this._services.ContainsKey(type))
        {
            return null;
        }

        var loggerArgType = type.GenericTypeArguments.FirstOrDefault();

        using var nullLoggerFactory = new NullLoggerFactory();

        if (loggerArgType == null)
        {
            return nullLoggerFactory.CreateLogger(string.Empty);
        }

        var loggerType = typeof(NullLogger<>).MakeGenericType(loggerArgType);
        var logger = Activator.CreateInstance(loggerType);

        return logger;

        // var mockType = typeof(object).MakeGenericType(type);
        // var mockConstructor = mockType.GetConstructors().First();
        // var service = mockConstructor.Invoke(null);
        //
        // var mockedProperties = mockType.GetProperties();
        // var objectProperty = Array.Find(
        //     mockedProperties,
        //     propertyInfo => propertyInfo.Name == "Object" && propertyInfo.PropertyType == type);
        //
        // return objectProperty?.GetValue(service);
    }

    private object? GetOptions(
        Type type,
        ICollection<string> parentTypes)
    {
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(IOptions<>))
        {
            return null;
        }

        var typeForOptions = type.GetGenericArguments().First();
        var optionsWrapperType = typeof(OptionsWrapper<>).MakeGenericType(typeForOptions);

        parentTypes.Add(type.Name);

        return this.GetOrCreateService(optionsWrapperType, parentTypes);
    }

    private object? GetBlueprintDependency(
        Type dependencyType,
        ICollection<string> parentTypes)
    {
        // If the test target builder has been customised with the dependency type use that instead of the default blueprint.
        if (this._blueprints.TryGetValue(dependencyType, out var existingBlueprintDependency))
        {
            return BuildDependencyFromBlueprint(existingBlueprintDependency);
        }

        var entryPointAssembly = EntryPointAssembly.Load();

        var blueprintAssemblies = entryPointAssembly.GetCustomAttributes<BlueprintAssemblyAttribute>()
            .Select(
                blueprintAssemblyAttribute => Assembly.Load(blueprintAssemblyAttribute.Name)
                                              ?? throw new BlueprintDependencyException(
                                                  $"Unable to load assembly {blueprintAssemblyAttribute.Name}. Ensure it is referenced in the project."))
            .ToList();

        if (!blueprintAssemblies.Any())
        {
            blueprintAssemblies = [entryPointAssembly];
        }

        var blueprintDependency = blueprintAssemblies
            .SelectMany(assembly => assembly.GetExportedTypes())
            .Where(
                type => type.BaseType != null && type.BaseType.IsGenericType &&
                        type.BaseType.GetGenericTypeDefinition() == typeof(BlueprintDependency<>))
            .Where(type => type.BaseType.GetGenericArguments()[0] == dependencyType) // Match the generic type argument
            .Select(Activator.CreateInstance)
            .FirstOrDefault();

        if (blueprintDependency == null)
        {
            return null;
        }

        parentTypes.Add(dependencyType.Name);

        return BuildDependencyFromBlueprint(blueprintDependency);
    }

    private IBlueprintDependency<TDependency>? FindBlueprintDependency<TDependency>()
        where TDependency : class
    {
        var dependencyType = typeof(TDependency);

        if (this._blueprints.TryGetValue(dependencyType, out var existingBlueprintDependency))
        {
            return existingBlueprintDependency as IBlueprintDependency<TDependency>;
        }

        var entryPointAssembly = EntryPointAssembly.Load();

        var blueprintAssemblies = entryPointAssembly.GetCustomAttributes<BlueprintAssemblyAttribute>()
            .Select(
                blueprintAssemblyAttribute => Assembly.Load(blueprintAssemblyAttribute.Name)
                                              ?? throw new BlueprintDependencyException(
                                                  $"Unable to load assembly {blueprintAssemblyAttribute.Name}. Ensure it is referenced in the project."))
            .ToList();

        if (!blueprintAssemblies.Any())
        {
            blueprintAssemblies = [entryPointAssembly];
        }

        var blueprintDependency = blueprintAssemblies
            .SelectMany(assembly => assembly.GetExportedTypes())
            .Where(
                type => type.BaseType != null && type.BaseType.IsGenericType &&
                        type.BaseType.GetGenericTypeDefinition() == typeof(BlueprintDependency<>))
            .Where(type => type.BaseType?.GetGenericArguments()[0] == dependencyType) // Match the generic type argument
            .Select(Activator.CreateInstance)
            .FirstOrDefault();

        return blueprintDependency as IBlueprintDependency<TDependency>;
    }
}
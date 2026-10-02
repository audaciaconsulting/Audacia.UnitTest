using System.Reflection;
using System.Runtime.CompilerServices;
using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// Extensions to <see cref="TestTargetBuilder"/> for testing code that sends requests through <see cref="IMediator"/>.
/// </summary>
public static class TestTargetBuilderMediatorExtensions
{
    private static readonly ConditionalWeakTable<TestTargetBuilder, MediatorTestConfiguration> Configurations = [];

    /// <summary>
    /// Provides a real <see cref="IMediator"/> that dispatches to the handlers in the given assemblies, so a test target
    /// that depends on it can send requests through the real pipeline. Pipeline behaviours can then be added with
    /// <see cref="AddPipelineBehavior(TestTargetBuilder, Type)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Handlers are built by the <paramref name="builder"/>, so the services they depend on can be supplied with
    /// <see cref="TestTargetBuilder.With{TDependency}"/>, <see cref="TestTargetBuilder.WithOptions{TOptions}"/> and so on.
    /// </para>
    /// </remarks>
    /// <param name="builder">The builder to provide the mediator to.</param>
    /// <param name="handlerAssemblies">The assemblies to scan for request handlers.</param>
    /// <returns>The builder.</returns>
    public static TestTargetBuilder WithMediator(this TestTargetBuilder builder, params Assembly[] handlerAssemblies)
    {
        return builder.WithMediator(configuration => configuration.AddHandlers(handlerAssemblies));
    }

    /// <summary>
    /// Provides a real <see cref="IMediator"/> configured with handlers and pipeline behaviours, so a test
    /// target that depends on it can send requests through the real pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mediator is created when the first request is sent, so handlers, behaviours and the services they depend on
    /// can be added before or after this call, up to that point.
    /// </para>
    /// <para>
    /// Handlers and behaviours are built by the <paramref name="builder"/> when they are first needed.
    /// </para>
    /// <para>
    /// A behaviour that depends on <c>IEnumerable&lt;T&gt;</c> of an interface, such as the validators for a request,
    /// is given the implementations of that interface found in the handler assemblies, built as they are needed.
    /// </para>
    /// <para>
    /// This can be called once per builder.
    /// </para>
    /// </remarks>
    /// <param name="builder">The builder to provide the mediator to.</param>
    /// <param name="configure">Configures the mediator.</param>
    /// <returns>The builder.</returns>
    /// <exception cref="TestTargetBuilderException">If an <see cref="IMediator"/> has already been supplied to the builder.</exception>
    public static TestTargetBuilder WithMediator(
        this TestTargetBuilder builder,
        Action<MediatorTestConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var configuration = new MediatorTestConfiguration();
        configuration.AddHandlers();
        configure(configuration);

        builder.With<IMediator>(new DeferredMediator(() => CreateMediator(builder, configuration)));
        Configurations.Add(builder, configuration);

        return builder;
    }

    /// <summary>
    /// Adds a pipeline behaviour to the mediator provided with <c>WithMediator</c>. The behaviour is built by the
    /// <paramref name="builder"/>, so its own dependencies are resolved or supplied in the same way as any other.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Behaviours run in the order they are added, with the first one added being the outermost.
    /// </para>
    /// </remarks>
    /// <param name="builder">The builder the mediator was provided to.</param>
    /// <param name="behaviorType">
    /// The behaviour, which may be an open generic such as <c>typeof(ValidationBehavior&lt;,&gt;)</c>. An open generic
    /// behaviour must take the request type and then the response type as its type parameters.
    /// </param>
    /// <returns>The builder.</returns>
    /// <exception cref="InvalidOperationException">If <c>WithMediator</c> has not been called, or the mediator has already handled a request.</exception>
    /// <exception cref="ArgumentException">If <paramref name="behaviorType"/> is not a pipeline behaviour.</exception>
    public static TestTargetBuilder AddPipelineBehavior(this TestTargetBuilder builder, Type behaviorType)
    {
        GetConfiguration(builder).AddPipelineBehavior(behaviorType);

        return builder;
    }

    /// <summary>
    /// Adds a pipeline behaviour to the mediator provided with <c>WithMediator</c>.
    /// </summary>
    /// <param name="builder">The builder the mediator was provided to.</param>
    /// <typeparam name="TBehavior">The behaviour. Use the <see cref="Type"/> overload for an open generic.</typeparam>
    /// <returns>The builder.</returns>
    /// <exception cref="InvalidOperationException">If <c>WithMediator</c> has not been called, or the mediator has already handled a request.</exception>
    public static TestTargetBuilder AddPipelineBehavior<TBehavior>(this TestTargetBuilder builder)
        where TBehavior : class
    {
        return builder.AddPipelineBehavior(typeof(TBehavior));
    }

    /// <summary>
    /// Adds a pipeline behaviour written as a delegate to the mediator provided with <c>WithMediator</c>, which runs
    /// around requests of type <typeparamref name="TRequest"/>.
    /// </summary>
    /// <param name="builder">The builder the mediator was provided to.</param>
    /// <param name="behavior">
    /// The delegate to run. Call the continuation to run the rest of the pipeline, or return without calling it to
    /// short-circuit.
    /// </param>
    /// <typeparam name="TRequest">The type of the request the behaviour applies to.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <returns>The builder.</returns>
    /// <exception cref="InvalidOperationException">If <c>WithMediator</c> has not been called, or the mediator has already handled a request.</exception>
    public static TestTargetBuilder AddPipelineBehavior<TRequest, TResponse>(
        this TestTargetBuilder builder,
        Func<TRequest, RequestHandlerContinuation<TResponse>, CancellationToken, Task<TResponse>> behavior)
        where TRequest : IRequest<TResponse>
    {
        GetConfiguration(builder).AddPipelineBehavior(behavior);

        return builder;
    }

    /// <summary>
    /// Replaces the handler for requests of type <typeparamref name="TRequest"/> in the mediator provided with
    /// <c>WithMediator</c>, so a request sent by the test target, or by another handler, is handled by the given
    /// <paramref name="handler"/> instead. This works whether or not a real handler was found, and pipeline behaviours
    /// still run around it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a handler is provided for the same request more than once, the last one is used.
    /// </para>
    /// </remarks>
    /// <param name="builder">The builder the mediator was provided to.</param>
    /// <param name="handler">The handler to use, for example a substitute.</param>
    /// <typeparam name="TRequest">The type of the request the handler handles.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <returns>The builder.</returns>
    /// <exception cref="InvalidOperationException">If <c>WithMediator</c> has not been called, or the mediator has already handled a request.</exception>
    public static TestTargetBuilder WithHandler<TRequest, TResponse>(
        this TestTargetBuilder builder,
        IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        GetConfiguration(builder).WithHandler(handler);

        return builder;
    }

    /// <summary>
    /// Replaces the handler for requests of type <typeparamref name="TRequest"/> in the mediator provided with
    /// <c>WithMediator</c> with a function that produces the response, so the real handler, and everything it depends on,
    /// is not used. Pipeline behaviours still run around it.
    /// </summary>
    /// <param name="builder">The builder the mediator was provided to.</param>
    /// <param name="response">Produces the response for a request.</param>
    /// <typeparam name="TRequest">The type of the request to fake the handling of.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <returns>The builder.</returns>
    /// <exception cref="InvalidOperationException">If <c>WithMediator</c> has not been called, or the mediator has already handled a request.</exception>
    public static TestTargetBuilder WithResponse<TRequest, TResponse>(
        this TestTargetBuilder builder,
        Func<TRequest, TResponse> response)
        where TRequest : IRequest<TResponse>
    {
        GetConfiguration(builder).WithResponse(response);

        return builder;
    }

    /// <summary>
    /// Replaces the handler for requests of type <typeparamref name="TRequest"/> in the mediator provided with
    /// <c>WithMediator</c> with an asynchronous function that produces the response, so the real handler, and everything
    /// it depends on, is not used. Pipeline behaviours still run around it.
    /// </summary>
    /// <param name="builder">The builder the mediator was provided to.</param>
    /// <param name="response">Produces the response for a request.</param>
    /// <typeparam name="TRequest">The type of the request to fake the handling of.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <returns>The builder.</returns>
    /// <exception cref="InvalidOperationException">If <c>WithMediator</c> has not been called, or the mediator has already handled a request.</exception>
    public static TestTargetBuilder WithResponse<TRequest, TResponse>(
        this TestTargetBuilder builder,
        Func<TRequest, CancellationToken, Task<TResponse>> response)
        where TRequest : IRequest<TResponse>
    {
        GetConfiguration(builder).WithResponse(response);

        return builder;
    }

    private static MediatorTestConfiguration GetConfiguration(TestTargetBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return Configurations.TryGetValue(builder, out var configuration)
            ? configuration
            : throw new InvalidOperationException(
                "Call WithMediator before adding pipeline behaviors or providing handlers.");
    }

    private static HandlerCheckingMediator CreateMediator(TestTargetBuilder builder, MediatorTestConfiguration configuration)
    {
        configuration.IsBuilt = true;
        configuration.ApplyHandlerOverrides();

        var handlers = FindHandlers(configuration.Services);
#pragma warning disable IDISP001 // The provider only creates the mediator, and lives as long as the builder's services.
        var provider = BuildProvider(builder, configuration, handlers);
#pragma warning restore IDISP001

        return new HandlerCheckingMediator(
            provider.GetRequiredService<IMediator>(),
            [.. handlers.Select(handler => handler.Request)]);
    }

    private static List<(Type Request, Type Response)> FindHandlers(IServiceCollection services)
    {
        return
        [
            .. services
                .Select(descriptor => descriptor.ServiceType)
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(type => (Request: type.GetGenericArguments()[0], Response: type.GetGenericArguments()[1]))
        ];
    }

    /// <summary>
    /// Replaces how each handler and behaviour is created, so the <paramref name="builder"/> builds them instead of the
    /// container. Open generic behaviours are closed over every request that has a handler, as the container cannot
    /// create open generics from a factory.
    /// </summary>
    private static ServiceProvider BuildProvider(
        TestTargetBuilder builder,
        MediatorTestConfiguration configuration,
        List<(Type Request, Type Response)> handlers)
    {
        IServiceCollection services = new ServiceCollection();
        var supplied = new HashSet<Type>();
        var context = new DescriptorBuildContext(builder, configuration, handlers, services, supplied);

        foreach (var descriptor in configuration.Services)
        {
            AddDescriptor(context, descriptor);
        }

        return services.BuildServiceProvider();
    }

    private static void AddDescriptor(
        DescriptorBuildContext context,
        ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationType is null || descriptor.ServiceType == typeof(IMediator))
        {
            context.Services.Add(descriptor);
            return;
        }

        foreach (var (serviceType, implementationType) in Close(descriptor, context.Handlers))
        {
            context.Services.Add(
                new ServiceDescriptor(
                    serviceType,
                    _ => BuildWithCollections(
                        context.Builder,
                        implementationType,
                        context.Configuration.HandlerAssemblies,
                        context.Supplied),
                    descriptor.Lifetime));
        }
    }

    private static object BuildWithCollections(
        TestTargetBuilder builder,
        Type implementationType,
        List<Assembly> assemblies,
        HashSet<Type> supplied)
    {
        SupplyCollectionDependencies(builder, implementationType, assemblies, supplied);

        return builder.Build(implementationType);
    }

    private static List<(Type ServiceType, Type ImplementationType)> Close(
        ServiceDescriptor descriptor,
        List<(Type Request, Type Response)> handlers)
    {
        if (!descriptor.ServiceType.IsGenericTypeDefinition)
        {
            return [(descriptor.ServiceType, descriptor.ImplementationType!)];
        }

        var closed = new List<(Type ServiceType, Type ImplementationType)>();
        foreach (var (request, response) in handlers)
        {
            try
            {
                closed.Add((
                    descriptor.ServiceType.MakeGenericType(request, response),
                    descriptor.ImplementationType!.MakeGenericType(request, response)));
            }
            catch (ArgumentException)
            {
                // The behaviour's constraints rule out this request, so it does not apply to it.
            }
        }

        return closed;
    }

    /// <summary>
    /// The <see cref="TestTargetBuilder"/> cannot build a collection, so when a type depends on an
    /// <c>IEnumerable&lt;T&gt;</c> of an interface, such as the validators for a request, supply the implementations of
    /// that interface found in the handler assemblies. They are built when first used, so only the validators a test
    /// needs are created, and anything supplied by the test itself is left alone.
    /// </summary>
    private static void SupplyCollectionDependencies(
        TestTargetBuilder builder,
        Type implementationType,
        List<Assembly> assemblies,
        HashSet<Type> supplied)
    {
        var supply = typeof(TestTargetBuilderMediatorExtensions)
            .GetMethod(nameof(SupplyImplementationsOf), BindingFlags.NonPublic | BindingFlags.Static)!;

        var itemTypes = implementationType.GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(type => type.GetGenericArguments()[0])
            .Where(itemType => itemType.IsInterface);

        // Requests can be sent at the same time, and one must not build its behaviour until the collections it depends
        // on have been supplied, so the check and the supply happen together.
        lock (supplied)
        {
            foreach (var itemType in itemTypes)
            {
                if (supplied.Add(itemType))
                {
                    supply.MakeGenericMethod(itemType).Invoke(null, [builder, assemblies]);
                }
            }
        }
    }

    private static void SupplyImplementationsOf<TService>(TestTargetBuilder builder, List<Assembly> assemblies)
    {
        var implementations = assemblies
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                && typeof(TService).IsAssignableFrom(type))
            .ToList();

        try
        {
            builder.With<IEnumerable<TService>>(
                new LazyEnumerable<TService>(
                    () => [.. implementations.Select(implementation => (TService)builder.Build(implementation))]));
        }
        catch (TestTargetBuilderException)
        {
            // The test supplied its own, which is used instead.
        }
    }
}

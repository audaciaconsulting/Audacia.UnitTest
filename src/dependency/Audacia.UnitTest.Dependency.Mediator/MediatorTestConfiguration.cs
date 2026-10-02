using System.Reflection;
using Audacia.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// Describes the mediator a test sends requests through: which handlers it dispatches to and which pipeline behaviours
/// run around them.
/// </summary>
public sealed class MediatorTestConfiguration
{
    private readonly Dictionary<Type, object> _handlerOverrides = [];

    /// <summary>
    /// Gets the registrations made by the configuration, which the mediator is then built from.
    /// </summary>
    internal ServiceCollection Services { get; } = new();

    /// <summary>
    /// Gets the assemblies handlers were added from, which are also searched for the collections behaviours depend on.
    /// </summary>
    internal List<Assembly> HandlerAssemblies { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the mediator has been created, after which nothing more can be added.
    /// </summary>
    internal bool IsBuilt { get; set; }

    /// <summary>
    /// Adds every request handler found in the given <paramref name="handlerAssemblies"/>.
    /// </summary>
    /// <param name="handlerAssemblies">The assemblies to scan for implementations of <see cref="IRequestHandler{TRequest,TResponse}"/>.</param>
    /// <returns>The configuration.</returns>
    public MediatorTestConfiguration AddHandlers(params Assembly[] handlerAssemblies)
    {
        EnsureNotBuilt();
        HandlerAssemblies.AddRange(handlerAssemblies);
        Services.AddMediator(handlerAssemblies);

        return this;
    }

    /// <summary>
    /// Adds a pipeline behaviour that runs around the requests it applies to. The behaviour is built by the
    /// <see cref="TestTargetBuilder"/>, so its own dependencies are resolved or supplied in the same way as any other.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Behaviours run in the order they are added, with the first one added being the outermost.
    /// </para>
    /// </remarks>
    /// <param name="behaviorType">
    /// The behaviour, which may be an open generic such as <c>typeof(ValidationBehavior&lt;,&gt;)</c>. An open generic
    /// behaviour must take the request type and then the response type as its type parameters.
    /// </param>
    /// <returns>The configuration.</returns>
    /// <exception cref="ArgumentException">If <paramref name="behaviorType"/> is not a pipeline behaviour.</exception>
    public MediatorTestConfiguration AddPipelineBehavior(Type behaviorType)
    {
        EnsureNotBuilt();
        Services.AddPipelineBehavior(behaviorType);

        return this;
    }

    /// <summary>
    /// Adds a pipeline behaviour that runs around the requests it applies to.
    /// </summary>
    /// <typeparam name="TBehavior">The behaviour. Use the <see cref="Type"/> overload for an open generic.</typeparam>
    /// <returns>The configuration.</returns>
    public MediatorTestConfiguration AddPipelineBehavior<TBehavior>()
        where TBehavior : class
    {
        return AddPipelineBehavior(typeof(TBehavior));
    }

    /// <summary>
    /// Adds a pipeline behaviour written as a delegate, which runs around requests of type <typeparamref name="TRequest"/>.
    /// </summary>
    /// <param name="behavior">
    /// The delegate to run. Call the continuation to run the rest of the pipeline, or return without calling it to
    /// short-circuit.
    /// </param>
    /// <typeparam name="TRequest">The type of the request the behaviour applies to.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <returns>The configuration.</returns>
    public MediatorTestConfiguration AddPipelineBehavior<TRequest, TResponse>(
        Func<TRequest, RequestHandlerContinuation<TResponse>, CancellationToken, Task<TResponse>> behavior)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(behavior);
        EnsureNotBuilt();

        Services.AddSingleton<IPipelineBehavior<TRequest, TResponse>>(
            new DelegatePipelineBehavior<TRequest, TResponse>(behavior));

        return this;
    }

    /// <summary>
    /// Replaces the handler for requests of type <typeparamref name="TRequest"/> with the given
    /// <paramref name="handler"/>, whether or not a real handler was found. Pipeline behaviours still run around it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a handler is provided for the same request more than once, the last one is used, wherever the real handlers were
    /// added.
    /// </para>
    /// </remarks>
    /// <param name="handler">The handler to use, for example a substitute.</param>
    /// <typeparam name="TRequest">The type of the request the handler handles.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <returns>The configuration.</returns>
    public MediatorTestConfiguration WithHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(handler);
        EnsureNotBuilt();

        _handlerOverrides[typeof(IRequestHandler<TRequest, TResponse>)] = handler;

        return this;
    }

    /// <summary>
    /// Replaces the handler for requests of type <typeparamref name="TRequest"/> with a function that produces the
    /// response, so the real handler, and everything it depends on, is not used.
    /// </summary>
    /// <param name="response">Produces the response for a request.</param>
    /// <typeparam name="TRequest">The type of the request to fake the handling of.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <returns>The configuration.</returns>
    public MediatorTestConfiguration WithResponse<TRequest, TResponse>(Func<TRequest, TResponse> response)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(response);

        return WithResponse<TRequest, TResponse>((request, _) => Task.FromResult(response(request)));
    }

    /// <summary>
    /// Replaces the handler for requests of type <typeparamref name="TRequest"/> with an asynchronous function that
    /// produces the response, so the real handler, and everything it depends on, is not used.
    /// </summary>
    /// <param name="response">Produces the response for a request.</param>
    /// <typeparam name="TRequest">The type of the request to fake the handling of.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <returns>The configuration.</returns>
    public MediatorTestConfiguration WithResponse<TRequest, TResponse>(
        Func<TRequest, CancellationToken, Task<TResponse>> response)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(response);

        return WithHandler(new DelegateRequestHandler<TRequest, TResponse>(response));
    }

    /// <summary>
    /// Replaces the registered handlers with any that were provided to stand in for them. This is done when the mediator
    /// is created, so the result does not depend on the order things were added in.
    /// </summary>
    internal void ApplyHandlerOverrides()
    {
        foreach (var (serviceType, handler) in _handlerOverrides)
        {
            foreach (var registered in Services.Where(descriptor => descriptor.ServiceType == serviceType).ToList())
            {
                Services.Remove(registered);
            }

            Services.AddSingleton(serviceType, handler);
        }
    }

    private void EnsureNotBuilt()
    {
        if (IsBuilt)
        {
            throw new InvalidOperationException(
                "The mediator has already handled a request, so handlers and behaviours can no longer be added.");
        }
    }
}

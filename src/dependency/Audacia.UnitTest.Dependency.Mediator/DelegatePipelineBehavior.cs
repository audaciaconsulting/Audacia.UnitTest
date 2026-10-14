using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// A pipeline behaviour that runs a delegate, for tests that need a step in the pipeline without writing a class.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
/// <param name="behavior">The delegate to run around the rest of the pipeline.</param>
internal sealed class DelegatePipelineBehavior<TRequest, TResponse>(
    Func<TRequest, RequestHandlerContinuation<TResponse>, CancellationToken, Task<TResponse>> behavior)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc/>
    public Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerContinuation<TResponse> continuation,
        CancellationToken cancellationToken)
    {
        return behavior(request, continuation, cancellationToken);
    }
}

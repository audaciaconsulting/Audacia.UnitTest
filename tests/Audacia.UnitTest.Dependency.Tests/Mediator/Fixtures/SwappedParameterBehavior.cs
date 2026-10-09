using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures;

/// <summary>
/// An open generic behaviour that takes the response type before the request type, so it cannot be closed.
/// </summary>
public sealed class SwappedParameterBehavior<TResponse, TRequest> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerContinuation<TResponse> continuation,
        CancellationToken cancellationToken)
    {
        return continuation(cancellationToken);
    }
}

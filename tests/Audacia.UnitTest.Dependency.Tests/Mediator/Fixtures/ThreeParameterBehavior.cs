using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures;

/// <summary>
/// An open generic behaviour with a type parameter more than the request and response, so it cannot be closed.
/// </summary>
public sealed class ThreeParameterBehavior<TRequest, TResponse, TExtra> : IPipelineBehavior<TRequest, TResponse>
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

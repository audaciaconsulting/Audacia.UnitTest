using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures;

/// <summary>
/// Records the name of each request it runs around, and applies to every request.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(PipelineLog log) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerContinuation<TResponse> continuation,
        CancellationToken cancellationToken)
    {
        log.Add(typeof(TRequest).Name);

        return continuation(cancellationToken);
    }
}

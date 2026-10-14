using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// A request handler that runs a delegate, for tests that need to fake the handling of a request without writing a class.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
/// <param name="handler">The delegate that produces the response.</param>
internal sealed class DelegateRequestHandler<TRequest, TResponse>(
    Func<TRequest, CancellationToken, Task<TResponse>> handler)
    : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc/>
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        return handler(request, cancellationToken);
    }
}

using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// An <see cref="IMediator"/> that explains which request has no handler, rather than leaving the container to report
/// a missing service.
/// </summary>
/// <param name="inner">The mediator that handles requests.</param>
/// <param name="handledRequests">The request types that have a handler.</param>
internal sealed class HandlerCheckingMediator(IMediator inner, IReadOnlyCollection<Type> handledRequests) : IMediator
{
    /// <inheritdoc/>
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return handledRequests.Contains(request.GetType())
            ? inner.SendAsync(request, cancellationToken)
            : throw new InvalidOperationException(
                $"There is no handler for '{request.GetType().Name}'. Pass the assembly containing it to WithMediator, "
                + "or provide one via WithHandler or WithResponse.");
    }
}

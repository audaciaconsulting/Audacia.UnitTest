using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// An <see cref="IMediator"/> that creates the real one when the first request is sent, so handlers and behaviours can
/// keep being added to the <see cref="TestTargetBuilder"/> until then.
/// </summary>
/// <param name="create">Creates the real mediator.</param>
internal sealed class DeferredMediator(Func<IMediator> create) : IMediator
{
    private readonly Lazy<IMediator> _mediator = new(create);

    /// <inheritdoc/>
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        return _mediator.Value.SendAsync(request, cancellationToken);
    }
}

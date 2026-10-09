using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Orders;

/// <summary>
/// A handler that sends another request, so tests can fake or use the real handler for the request it sends.
/// </summary>
public sealed class PlaceOrderHandler(IMediator mediator) : IRequestHandler<PlaceOrder, string>
{
    public async Task<string> HandleAsync(PlaceOrder request, CancellationToken cancellationToken)
    {
        var receipt = await mediator.SendAsync(new SendReceipt(request.Customer), cancellationToken);

        return $"Order for {request.Customer}: {receipt}";
    }
}

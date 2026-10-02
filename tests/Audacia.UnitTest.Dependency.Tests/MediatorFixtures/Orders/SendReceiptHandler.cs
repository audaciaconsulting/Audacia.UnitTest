using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Orders;

public sealed class SendReceiptHandler(IReceiptSender sender) : IRequestHandler<SendReceipt, string>
{
    public Task<string> HandleAsync(SendReceipt request, CancellationToken cancellationToken)
    {
        return Task.FromResult(sender.Send(request.Customer));
    }
}

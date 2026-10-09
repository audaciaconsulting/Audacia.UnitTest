using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Orders;

public sealed record SendReceipt(string Customer) : IRequest<string>;

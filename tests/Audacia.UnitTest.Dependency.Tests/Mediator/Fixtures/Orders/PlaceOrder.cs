using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Orders;

public sealed record PlaceOrder(string Customer) : IRequest<string>;

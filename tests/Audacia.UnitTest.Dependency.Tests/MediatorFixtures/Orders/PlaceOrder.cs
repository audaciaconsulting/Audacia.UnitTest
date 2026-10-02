using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Orders;

public sealed record PlaceOrder(string Customer) : IRequest<string>;

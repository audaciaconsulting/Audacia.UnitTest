using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Orders;

public sealed record SendReceipt(string Customer) : IRequest<string>;

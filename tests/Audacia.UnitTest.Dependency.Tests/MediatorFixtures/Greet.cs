using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

public sealed record Greet(string Name) : IRequest<string>;

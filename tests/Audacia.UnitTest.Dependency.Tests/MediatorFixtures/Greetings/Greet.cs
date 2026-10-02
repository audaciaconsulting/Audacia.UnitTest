using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Greetings;

public sealed record Greet(string Name) : IRequest<string>;

using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Greetings;

public sealed record Greet(string Name) : IRequest<string>;

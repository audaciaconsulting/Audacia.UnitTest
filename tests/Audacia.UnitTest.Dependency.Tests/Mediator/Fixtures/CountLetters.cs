using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures;

public sealed record CountLetters(string Name) : IRequest<int>;

using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

public sealed record CountLetters(string Name) : IRequest<int>;

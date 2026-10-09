using Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Greetings;

public sealed class UpperCaseGreetingFormatter : IGreetingFormatter
{
    public string Format(string name)
    {
        return $"HELLO {name.ToUpperInvariant()}";
    }
}

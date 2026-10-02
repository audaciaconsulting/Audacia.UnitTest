using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Greetings;

public sealed class UpperCaseGreetingFormatter : IGreetingFormatter
{
    public string Format(string name)
    {
        return $"HELLO {name.ToUpperInvariant()}";
    }
}

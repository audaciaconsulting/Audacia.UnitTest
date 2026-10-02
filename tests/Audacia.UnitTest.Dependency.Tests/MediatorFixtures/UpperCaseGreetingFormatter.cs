namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

public sealed class UpperCaseGreetingFormatter : IGreetingFormatter
{
    public string Format(string name)
    {
        return $"HELLO {name.ToUpperInvariant()}";
    }
}

using Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Greetings;

public sealed class GreetNameIsNotBlank : IValidates<Greet>
{
    public IEnumerable<string> Problems(Greet value)
    {
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            yield return "A name is required";
        }
    }
}

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

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

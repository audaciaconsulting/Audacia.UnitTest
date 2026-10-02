namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

public sealed class GreetNameIsNotBanned(IBannedNames bannedNames) : IValidates<Greet>
{
    public IEnumerable<string> Problems(Greet value)
    {
        if (bannedNames.IsBanned(value.Name))
        {
            yield return "That name is banned";
        }
    }
}

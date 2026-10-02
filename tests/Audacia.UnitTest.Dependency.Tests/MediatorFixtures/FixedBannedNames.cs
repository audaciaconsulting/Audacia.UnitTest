using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

public sealed class FixedBannedNames(params string[] names) : IBannedNames
{
    public bool IsBanned(string name)
    {
        return names.Contains(name);
    }
}

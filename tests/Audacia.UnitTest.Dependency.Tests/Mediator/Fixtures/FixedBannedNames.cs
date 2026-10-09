using Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures;

public sealed class FixedBannedNames(params string[] names) : IBannedNames
{
    public bool IsBanned(string name)
    {
        return names.Contains(name);
    }
}

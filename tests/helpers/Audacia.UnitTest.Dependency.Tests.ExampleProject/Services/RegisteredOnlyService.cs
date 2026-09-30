namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Services;

internal sealed class RegisteredOnlyService(IUnregisteredPart part) : IRegisteredOnlyService
{
    public string Describe()
    {
        return $"Built with {part.Name}";
    }
}

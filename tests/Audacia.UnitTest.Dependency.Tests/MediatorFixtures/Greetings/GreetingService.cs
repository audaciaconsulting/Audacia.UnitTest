using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Greetings;

public sealed class GreetingService(IMediator mediator)
{
    public Task<string> GreetAsync(string name)
    {
        return mediator.SendAsync(new Greet(name), CancellationToken.None);
    }
}

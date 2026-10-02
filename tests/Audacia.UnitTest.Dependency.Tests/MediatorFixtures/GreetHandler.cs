using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

public sealed class GreetHandler(IGreetingFormatter formatter) : IRequestHandler<Greet, string>
{
    public Task<string> HandleAsync(Greet request, CancellationToken cancellationToken)
    {
        return Task.FromResult(formatter.Format(request.Name));
    }
}

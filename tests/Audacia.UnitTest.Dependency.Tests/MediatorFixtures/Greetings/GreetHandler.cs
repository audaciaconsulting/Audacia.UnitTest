using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Greetings;

public sealed class GreetHandler(IGreetingFormatter formatter) : IRequestHandler<Greet, string>
{
    public Task<string> HandleAsync(Greet request, CancellationToken cancellationToken)
    {
        return Task.FromResult(formatter.Format(request.Name));
    }
}

using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Greetings;

public sealed class GreetHandler(IGreetingFormatter formatter) : IRequestHandler<Greet, string>
{
    public Task<string> HandleAsync(Greet request, CancellationToken cancellationToken)
    {
        return Task.FromResult(formatter.Format(request.Name));
    }
}

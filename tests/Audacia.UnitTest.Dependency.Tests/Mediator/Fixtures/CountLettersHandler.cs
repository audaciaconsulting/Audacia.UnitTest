using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures;

public sealed class CountLettersHandler : IRequestHandler<CountLetters, int>
{
    public Task<int> HandleAsync(CountLetters request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Name.Length);
    }
}

using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

public sealed class CountLettersHandler : IRequestHandler<CountLetters, int>
{
    public Task<int> HandleAsync(CountLetters request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Name.Length);
    }
}

using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

/// <summary>
/// Short-circuits with the validation problems. Only applies to class responses, so it cannot apply to
/// <see cref="CountLetters"/>.
/// </summary>
public sealed class ValidatesBehavior<TRequest, TResponse>(IEnumerable<IValidates<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    public Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerContinuation<TResponse> continuation,
        CancellationToken cancellationToken)
    {
        var problems = validators.SelectMany(validator => validator.Problems(request)).ToList();

        return problems.Count > 0
            ? Task.FromResult((TResponse)(object)$"Invalid: {string.Join(", ", problems)}")
            : continuation(cancellationToken);
    }
}

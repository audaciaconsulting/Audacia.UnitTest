using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Mediator;
using Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures;
using Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Abstractions;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests.Mediator;

/// <summary>
/// Covers requests sent at the same time, which build handlers, behaviours and validators on a
/// <see cref="TestTargetBuilder"/> that is not thread-safe.
/// </summary>
public class ConcurrentMediatorTests
{
    private static readonly System.Reflection.Assembly TestAssembly = typeof(ConcurrentMediatorTests).Assembly;

    [Fact]
    public async Task Should_handle_requests_with_different_validator_collections_at_the_same_time()
    {
        const int RequestCount = 2;
        // Each builder is new, so the services it resolves and caches are created while the requests race.
        for (var attempt = 0; attempt < 50; attempt++)
        {
            // Arrange
            var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var start = new SemaphoreSlim(0, RequestCount);
            var arrivals = 0;

            async Task<string> WaitToValidateAsync(
                RequestHandlerContinuation<string> continuation,
                CancellationToken cancellationToken)
            {
                if (Interlocked.Increment(ref arrivals) == RequestCount)
                {
                    ready.SetResult();
                }

                await start.WaitAsync(cancellationToken);
                return await continuation(cancellationToken);
            }

            var mediator = new TestTargetBuilder()
                .WithMediator(TestAssembly)
                .WithResponse<FirstValidationRequest, string>(_ => "handled")
                .WithResponse<SecondValidationRequest, string>(_ => "handled")
                .AddPipelineBehavior<FirstValidationRequest, string>(
                    async (_, continuation, cancellationToken) => await WaitToValidateAsync(continuation, cancellationToken))
                .AddPipelineBehavior<SecondValidationRequest, string>(
                    async (_, continuation, cancellationToken) => await WaitToValidateAsync(continuation, cancellationToken))
                .AddPipelineBehavior(typeof(ValidatesBehavior<,>))
                .Build<IMediator>();

            // Act
            var sends = new[]
            {
                Task.Run(async () => await mediator.SendAsync(new FirstValidationRequest(), TestContext.Current.CancellationToken)),
                Task.Run(async () => await mediator.SendAsync(new SecondValidationRequest(), TestContext.Current.CancellationToken))
            };

            // Both pipelines are built before either lazy validator collection is enumerated.
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            start.Release(RequestCount);
            var responses = await Task.WhenAll(sends);

            // Assert
            responses[0].ShouldStartWith("Invalid: ");
            responses[1].ShouldBe(responses[0]);
        }
    }

    public sealed record FirstValidationRequest : IRequest<string>;

    public sealed record SecondValidationRequest : IRequest<string>;

    public sealed class SharedValidationDependency
    {
        public SharedValidationDependency()
        {
            // Widen the race window without waiting for another constructor, which the shared gate would block.
            Thread.Sleep(20);
        }

        public Guid Id { get; } = Guid.NewGuid();
    }

    public sealed class FirstRequestValidator(SharedValidationDependency dependency) : IValidates<FirstValidationRequest>
    {
        public IEnumerable<string> Problems(FirstValidationRequest value)
        {
            yield return dependency.Id.ToString();
        }
    }

    public sealed class SecondRequestValidator(SharedValidationDependency dependency) : IValidates<SecondValidationRequest>
    {
        public IEnumerable<string> Problems(SecondValidationRequest value)
        {
            yield return dependency.Id.ToString();
        }
    }
}

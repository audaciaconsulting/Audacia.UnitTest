using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Mediator;
using Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Greetings;
using NSubstitute;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests.Mediator;

/// <summary>
/// Covers <see cref="DeferredMediator"/>, to ensure that the real mediator is created lazily and only once.
/// </summary>
public class DeferredMediatorTests
{
    [Fact]
    public void Should_not_create_the_mediator_until_a_request_is_sent()
    {
        // Arrange
        var created = 0;

        // Act
        _ = new DeferredMediator(() =>
        {
            created++;
            return Substitute.For<IMediator>();
        });

        // Assert
        created.ShouldBe(0);
    }

    [Fact]
    public async Task Should_create_the_mediator_once_however_many_requests_are_sent()
    {
        // Arrange
        var created = 0;
        var deferred = new DeferredMediator(() =>
        {
            created++;
            return CreateInnerMediator();
        });

        // Act
        await deferred.SendAsync(new Greet("sam"), CancellationToken.None);
        await deferred.SendAsync(new Greet("alex"), CancellationToken.None);
        await deferred.SendAsync(new Greet("jo"), CancellationToken.None);

        // Assert
        created.ShouldBe(1);
    }

    [Fact]
    public async Task Should_create_the_mediator_once_when_requests_are_sent_at_the_same_time()
    {
        // Arrange
        var created = 0;
        var deferred = new DeferredMediator(() =>
        {
            Interlocked.Increment(ref created);
            Thread.Sleep(50);
            return CreateInnerMediator();
        });

        // Act
        var sends = Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => deferred.SendAsync(new Greet("sam"), CancellationToken.None)));
        await Task.WhenAll(sends);

        // Assert
        created.ShouldBe(1);
    }

    [Fact]
    public async Task Should_send_every_request_to_the_same_created_mediator()
    {
        // Arrange
        var inner = CreateInnerMediator();
        var deferred = new DeferredMediator(() => inner);
        var first = new Greet("sam");
        var second = new Greet("alex");
        using var cancellation = new CancellationTokenSource();

        // Act
        var firstResponse = await deferred.SendAsync(first, cancellation.Token);
        await deferred.SendAsync(second, CancellationToken.None);

        // Assert
        firstResponse.ShouldBe("response");
        await inner.Received(1).SendAsync(first, cancellation.Token);
        await inner.Received(1).SendAsync(second, CancellationToken.None);
    }

    [Fact]
    public void Should_provide_the_same_mediator_to_every_test_target_built_from_the_builder()
    {
        // Arrange
        var builder = new TestTargetBuilder().WithMediator(typeof(DeferredMediatorTests).Assembly);

        // Act
        var first = builder.Build<IMediator>();
        var second = builder.Build<IMediator>();

        // Assert
        first.ShouldBeSameAs(second);
    }

    private static IMediator CreateInnerMediator()
    {
        var inner = Substitute.For<IMediator>();
        inner.SendAsync(Arg.Any<IRequest<string>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult("response"));

        return inner;
    }
}

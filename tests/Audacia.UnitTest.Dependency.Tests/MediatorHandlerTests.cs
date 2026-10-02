using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Mediator;
using Audacia.UnitTest.Dependency.Tests.MediatorFixtures;
using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Abstractions;
using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Greetings;
using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Orders;
using NSubstitute;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

/// <summary>
/// Covers requests sent by handlers, and faking the handling of a request with <c>WithHandler</c> and
/// <c>WithResponse</c>.
/// </summary>
public class MediatorHandlerTests
{
    private static readonly System.Reflection.Assembly TestAssembly = typeof(MediatorHandlerTests).Assembly;

    [Fact]
    public async Task Should_send_a_request_from_one_real_handler_to_another_using_fake_dependencies()
    {
        // Arrange
        var sender = Substitute.For<IReceiptSender>();
        sender.Send("sam").Returns("receipt sent");
        var mediator = new TestTargetBuilder()
            .With(sender)
            .WithMediator(TestAssembly)
            .Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new PlaceOrder("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe("Order for sam: receipt sent");
    }

    [Fact]
    public async Task Should_fake_a_request_sent_by_a_real_handler_with_a_function()
    {
        // Arrange
        var mediator = new TestTargetBuilder()
            .WithMediator(TestAssembly)
            .WithResponse<SendReceipt, string>(request => $"fake receipt for {request.Customer}")
            .Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new PlaceOrder("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe("Order for sam: fake receipt for sam");
    }

    [Fact]
    public async Task Should_fake_a_request_sent_by_a_real_handler_with_an_asynchronous_function()
    {
        // Arrange
        CancellationToken? received = null;
        using var cancellation = new CancellationTokenSource();
        var mediator = new TestTargetBuilder()
            .WithMediator(TestAssembly)
            .WithResponse<SendReceipt, string>((request, token) =>
            {
                received = token;
                return Task.FromResult($"async receipt for {request.Customer}");
            })
            .Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new PlaceOrder("sam"), cancellation.Token);

        // Assert
        result.ShouldBe("Order for sam: async receipt for sam");
        received.ShouldBe(cancellation.Token);
    }

    [Fact]
    public async Task Should_fake_a_request_sent_by_a_real_handler_with_a_substitute_handler()
    {
        // Arrange
        var handler = Substitute.For<IRequestHandler<SendReceipt, string>>();
        handler.HandleAsync(Arg.Any<SendReceipt>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult("substitute receipt"));
        var mediator = new TestTargetBuilder()
            .WithMediator(TestAssembly)
            .WithHandler(handler)
            .Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new PlaceOrder("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe("Order for sam: substitute receipt");
        await handler.Received(1).HandleAsync(new SendReceipt("sam"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_not_build_the_dependencies_of_a_handler_that_has_been_faked()
    {
        // Arrange
        var builder = new TestTargetBuilder()
            .WithMediator(TestAssembly)
            .WithResponse<SendReceipt, string>(_ => "fake receipt");

        // Act
        var result = await builder.Build<IMediator>().SendAsync(new PlaceOrder("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe("Order for sam: fake receipt");
        builder.Resolutions.ShouldNotContain(resolution => resolution.ImplementationType == typeof(EmailReceiptSender));
    }

    [Fact]
    public async Task Should_use_the_real_handler_for_requests_that_have_not_been_faked()
    {
        // Arrange
        var mediator = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly)
            .WithResponse<SendReceipt, string>(_ => "fake receipt")
            .Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new Greet("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe("HELLO SAM");
    }

    [Fact]
    public async Task Should_use_the_last_response_provided_for_a_request()
    {
        // Arrange
        var mediator = new TestTargetBuilder()
            .WithMediator(TestAssembly)
            .WithResponse<SendReceipt, string>(_ => "first")
            .WithResponse<SendReceipt, string>(_ => "second")
            .Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new SendReceipt("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe("second");
    }

    [Fact]
    public async Task Should_use_a_response_provided_in_the_configuration_before_the_handlers_were_added()
    {
        // Arrange
        var mediator = new TestTargetBuilder()
            .WithMediator(configuration => configuration
                .WithResponse<SendReceipt, string>(_ => "fake receipt")
                .AddHandlers(TestAssembly))
            .Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new SendReceipt("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe("fake receipt");
    }

    [Fact]
    public async Task Should_handle_only_the_faked_requests_when_no_assemblies_are_passed()
    {
        // Arrange
        var mediator = new TestTargetBuilder()
            .WithMediator()
            .WithResponse<SendReceipt, string>(_ => "fake receipt")
            .Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new SendReceipt("sam"), CancellationToken.None);
        var sendUnhandled = () => mediator.SendAsync(new Greet("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe("fake receipt");
        var exception = await sendUnhandled.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain(nameof(Greet));
    }

    [Fact]
    public async Task Should_name_the_request_when_no_handler_has_been_added_for_it()
    {
        // Arrange
        var mediator = new TestTargetBuilder().WithMediator().Build<IMediator>();

        // Act
        var send = () => mediator.SendAsync(new Greet("sam"), CancellationToken.None);

        // Assert
        var exception = await send.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain(nameof(Greet));
    }

    [Fact]
    public async Task Should_run_pipeline_behaviours_around_a_faked_handler()
    {
        // Arrange
        var log = new PipelineLog();
        var mediator = new TestTargetBuilder()
            .With(log)
            .WithMediator()
            .AddPipelineBehavior(typeof(LoggingBehavior<,>))
            .WithResponse<SendReceipt, string>(_ => "fake receipt")
            .Build<IMediator>();

        // Act
        await mediator.SendAsync(new SendReceipt("sam"), CancellationToken.None);

        // Assert
        log.Entries.ShouldBe([nameof(SendReceipt)]);
    }

    [Fact]
    public void Should_throw_when_a_response_is_provided_before_the_mediator()
    {
        // Arrange
        var builder = new TestTargetBuilder();

        // Act
        var provide = () => builder.WithResponse<SendReceipt, string>(_ => "fake receipt");

        // Assert
        provide.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_throw_when_a_response_is_provided_after_a_request_has_been_sent()
    {
        // Arrange
        var builder = new TestTargetBuilder()
            .WithMediator()
            .WithResponse<SendReceipt, string>(_ => "fake receipt");
        await builder.Build<IMediator>().SendAsync(new SendReceipt("sam"), CancellationToken.None);

        // Act
        var provide = () => builder.WithResponse<SendReceipt, string>(_ => "later");

        // Assert
        provide.ShouldThrow<InvalidOperationException>();
    }
}

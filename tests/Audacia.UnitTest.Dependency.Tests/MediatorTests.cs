using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Exceptions;
using Audacia.UnitTest.Dependency.Mediator;
using Audacia.UnitTest.Dependency.Tests.MediatorFixtures;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

/// <summary>
/// Covers <c>Audacia.UnitTest.Dependency.Mediator</c>, so requests sent through the mediator reach handlers,
/// behaviours and validators built by the <see cref="TestTargetBuilder"/>.
/// </summary>
public class MediatorTests
{
    private static readonly System.Reflection.Assembly TestAssembly = typeof(MediatorTests).Assembly;

    [Fact]
    public async Task Should_register_a_behaviour_added_twice_to_the_builder_once()
    {
        // Arrange
        var log = new PipelineLog();
        var service = new TestTargetBuilder()
            .With(log)
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly)
            .AddPipelineBehavior(typeof(LoggingBehavior<,>))
            .AddPipelineBehavior(typeof(LoggingBehavior<,>))
            .Build<GreetingService>();

        // Act
        await service.GreetAsync("sam");

        // Assert
        log.Entries.ShouldBe(["Greet"]);
    }

    [Fact]
    public async Task Should_register_a_behaviour_added_twice_in_the_configuration_once()
    {
        // Arrange
        var log = new PipelineLog();
        var service = new TestTargetBuilder()
            .With(log)
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(mediator => mediator
                .AddHandlers(TestAssembly)
                .AddPipelineBehavior(typeof(LoggingBehavior<,>))
                .AddPipelineBehavior(typeof(LoggingBehavior<,>)))
            .Build<GreetingService>();

        // Act
        await service.GreetAsync("sam");

        // Assert
        log.Entries.ShouldBe(["Greet"]);
    }

    [Fact]
    public async Task Should_register_a_closed_behaviour_added_twice_once()
    {
        // Arrange
        var log = new PipelineLog();
        var service = new TestTargetBuilder()
            .With(log)
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly)
            .AddPipelineBehavior<LoggingBehavior<Greet, string>>()
            .AddPipelineBehavior<LoggingBehavior<Greet, string>>()
            .Build<GreetingService>();

        // Act
        await service.GreetAsync("sam");

        // Assert
        log.Entries.ShouldBe(["Greet"]);
    }

    [Fact]
    public async Task Should_run_a_behaviour_once_for_each_request_sent()
    {
        // Arrange
        var log = new PipelineLog();
        var service = new TestTargetBuilder()
            .With(log)
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly)
            .AddPipelineBehavior(typeof(LoggingBehavior<,>))
            .Build<GreetingService>();

        // Act
        await service.GreetAsync("sam");
        await service.GreetAsync("alex");
        await service.GreetAsync("jo");

        // Assert
        log.Entries.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Should_run_an_open_generic_behaviour_once_for_each_kind_of_request()
    {
        // Arrange
        var log = new PipelineLog();
        var builder = new TestTargetBuilder()
            .With(log)
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly)
            .AddPipelineBehavior(typeof(LoggingBehavior<,>));
        var mediator = builder.Build<IMediator>();

        // Act
        await mediator.SendAsync(new Greet("sam"), CancellationToken.None);
        await mediator.SendAsync(new CountLetters("sam"), CancellationToken.None);

        // Assert
        log.Entries.ShouldBe(["Greet", "CountLetters"]);
    }

    [Fact]
    public async Task Should_run_a_delegate_behaviour_once_for_each_request_sent()
    {
        // Arrange
        var runs = 0;
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly)
            .AddPipelineBehavior<Greet, string>((_, next, token) =>
            {
                runs++;
                return next(token);
            })
            .Build<GreetingService>();

        // Act
        await service.GreetAsync("sam");
        await service.GreetAsync("alex");

        // Assert
        runs.ShouldBe(2);
    }

    [Fact]
    public async Task Should_run_delegate_behaviours_added_to_the_builder_in_the_order_they_were_added()
    {
        // Arrange
        var log = new List<string>();
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly)
            .AddPipelineBehavior<Greet, string>(async (_, next, token) =>
            {
                log.Add("outer before");
                var response = await next(token);
                log.Add("outer after");
                return response;
            })
            .AddPipelineBehavior<Greet, string>(async (_, next, token) =>
            {
                log.Add("inner before");
                return await next(token);
            })
            .Build<GreetingService>();

        // Act
        await service.GreetAsync("sam");

        // Assert
        log.ShouldBe(["outer before", "inner before", "outer after"]);
    }

    [Fact]
    public async Task Should_short_circuit_when_a_validator_reports_a_problem_for_a_behaviour_added_to_the_builder()
    {
        // Arrange
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .With<IBannedNames>(new FixedBannedNames("eve"))
            .WithMediator(TestAssembly)
            .AddPipelineBehavior(typeof(ValidatesBehavior<,>))
            .Build<GreetingService>();

        // Act
        var result = await service.GreetAsync("eve");

        // Assert
        result.ShouldBe("Invalid: That name is banned");
    }

    [Fact]
    public async Task Should_run_every_validator_for_the_request_for_a_behaviour_added_to_the_builder()
    {
        // Arrange
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .With<IBannedNames>(new FixedBannedNames(string.Empty))
            .WithMediator(TestAssembly)
            .AddPipelineBehavior(typeof(ValidatesBehavior<,>))
            .Build<GreetingService>();

        // Act
        var result = await service.GreetAsync(string.Empty);

        // Assert
        result.ShouldContain("A name is required");
        result.ShouldContain("That name is banned");
    }

    [Fact]
    public async Task Should_reach_the_handler_when_validators_find_no_problem_for_a_behaviour_added_to_the_builder()
    {
        // Arrange
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .With<IBannedNames>(new FixedBannedNames("eve"))
            .WithMediator(TestAssembly)
            .AddPipelineBehavior(typeof(ValidatesBehavior<,>))
            .Build<GreetingService>();

        // Act
        var result = await service.GreetAsync("sam");

        // Assert
        result.ShouldBe("HELLO SAM");
    }

    [Fact]
    public async Task Should_not_apply_an_open_generic_behaviour_added_to_the_builder_to_a_request_that_breaks_its_constraints()
    {
        // Arrange
        var builder = new TestTargetBuilder()
            .WithMediator(TestAssembly)
            .AddPipelineBehavior(typeof(ValidatesBehavior<,>));
        var mediator = builder.Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new CountLetters("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe(3);
    }

    [Fact]
    public async Task Should_run_a_generic_behaviour_added_to_the_builder_by_type_argument()
    {
        // Arrange
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .With<IBannedNames>(new FixedBannedNames("eve"))
            .WithMediator(TestAssembly)
            .AddPipelineBehavior<ValidatesBehavior<Greet, string>>()
            .Build<GreetingService>();

        // Act
        var result = await service.GreetAsync("eve");

        // Assert
        result.ShouldBe("Invalid: That name is banned");
    }

    [Fact]
    public void Should_reject_a_type_added_to_the_builder_that_is_not_a_pipeline_behaviour()
    {
        // Arrange
        var builder = new TestTargetBuilder().WithMediator(TestAssembly);

        // Act
        var add = () => builder.AddPipelineBehavior(typeof(GreetHandler));

        // Assert
        add.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Should_throw_when_a_behaviour_is_added_to_the_builder_before_the_mediator()
    {
        // Arrange
        var builder = new TestTargetBuilder();

        // Act
        var add = () => builder.AddPipelineBehavior(typeof(ValidatesBehavior<,>));

        // Assert
        add.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_throw_when_a_behaviour_is_added_to_the_builder_after_a_request_has_been_sent()
    {
        // Arrange
        var builder = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly);
        await builder.Build<GreetingService>().GreetAsync("sam");

        // Act
        var add = () => builder.AddPipelineBehavior(typeof(ValidatesBehavior<,>));

        // Assert
        add.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_use_a_behaviour_added_to_the_builder_when_dependencies_are_supplied_afterwards()
    {
        // Arrange
        var builder = new TestTargetBuilder()
            .WithMediator(TestAssembly)
            .AddPipelineBehavior(typeof(ValidatesBehavior<,>));
        builder.With<IGreetingFormatter>(new UpperCaseGreetingFormatter());
        builder.With<IBannedNames>(new FixedBannedNames("eve"));

        // Act
        var result = await builder.Build<GreetingService>().GreetAsync("eve");

        // Assert
        result.ShouldBe("Invalid: That name is banned");
    }

    [Fact]
    public async Task Should_send_a_request_to_a_handler_built_with_the_supplied_dependencies()
    {
        // Arrange
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(TestAssembly)
            .Build<GreetingService>();

        // Act
        var result = await service.GreetAsync("sam");

        // Assert
        result.ShouldBe("HELLO SAM");
    }

    [Fact]
    public async Task Should_use_a_dependency_supplied_after_the_mediator_was_configured()
    {
        // Arrange
        var builder = new TestTargetBuilder().WithMediator(TestAssembly);
        builder.With<IGreetingFormatter>(new UpperCaseGreetingFormatter());

        // Act
        var result = await builder.Build<GreetingService>().GreetAsync("sam");

        // Assert
        result.ShouldBe("HELLO SAM");
    }

    [Fact]
    public async Task Should_run_delegate_behaviours_in_the_order_they_were_added()
    {
        // Arrange
        var log = new List<string>();
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .WithMediator(mediator => mediator
                .AddHandlers(TestAssembly)
                .AddPipelineBehavior<Greet, string>(async (_, next, token) =>
                {
                    log.Add("outer before");
                    var response = await next(token);
                    log.Add("outer after");
                    return response;
                })
                .AddPipelineBehavior<Greet, string>(async (_, next, token) =>
                {
                    log.Add("inner before");
                    return await next(token);
                }))
            .Build<GreetingService>();

        // Act
        await service.GreetAsync("sam");

        // Assert
        log.ShouldBe(["outer before", "inner before", "outer after"]);
    }

    [Fact]
    public async Task Should_short_circuit_when_a_validator_reports_a_problem()
    {
        // Arrange
        var service = new TestTargetBuilder()
            .With<IGreetingFormatter>(new UpperCaseGreetingFormatter())
            .With<IBannedNames>(new FixedBannedNames("eve"))
            .WithMediator(mediator => mediator
                .AddHandlers(TestAssembly)
                .AddPipelineBehavior(typeof(ValidatesBehavior<,>)))
            .Build<GreetingService>();

        // Act
        var result = await service.GreetAsync("eve");

        // Assert
        result.ShouldBe("Invalid: That name is banned");
    }

    [Fact]
    public async Task Should_not_apply_an_open_generic_behaviour_to_a_request_that_breaks_its_constraints()
    {
        // Arrange
        var builder = new TestTargetBuilder()
            .WithMediator(mediator => mediator
                .AddHandlers(TestAssembly) // this is an alternative to WithMediator(TestAssembly) that allows you to configure the mediator further
                .AddPipelineBehavior(typeof(ValidatesBehavior<,>)));
        var mediator = builder.Build<IMediator>();

        // Act
        var result = await mediator.SendAsync(new CountLetters("sam"), CancellationToken.None);

        // Assert
        result.ShouldBe(3);
    }

    [Fact]
    public void Should_reject_a_behaviour_that_is_not_a_pipeline_behaviour()
    {
        // Arrange
        var builder = new TestTargetBuilder();

        // Act
        var configure = () => builder.WithMediator(mediator => mediator.AddPipelineBehavior(typeof(GreetHandler)));

        // Assert
        configure.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Should_throw_when_the_mediator_is_added_twice()
    {
        // Arrange
        var builder = new TestTargetBuilder().WithMediator(TestAssembly);

        // Act
        var addAgain = () => builder.WithMediator(TestAssembly);

        // Assert
        addAgain.ShouldThrow<TestTargetBuilderException>();
    }
}

using Audacia.Commands;
using Audacia.UnitTest.Dependency.Customisations;
using Audacia.UnitTest.Dependency.Exceptions;
using Audacia.UnitTest.Dependency.Http.Blueprints;
using Audacia.UnitTest.Dependency.Http.Builders;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Configuration;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Notifications;
using NSubstitute;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

public class TestTargetBuilderTests
{
    [Fact]
    public async Task Should_throw_exception_when_missing_no_implementation_can_be_found_for_dependency_of_interface_when_builder_target()
    {
        // Act
        var target = () => new TestTargetBuilder()
            .Build<AddAssetCommandHandler>();

        // Assert
        target.ShouldThrow<TestTargetBuilderException>();
    }

    [Fact]
    public async Task Should_be_able_to_create_target_with_http_client_factory_blueprint_dependency_with_okay_response()
    {
        // Arrange
        var addAssetCommand = new AddAssetCommand("Computer");
        var httpClientFactoryBlueprint = new HttpClientFactoryBlueprint();

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(httpClientFactoryBlueprint)
            .Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(addAssetCommand, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        Should_be_able_to_create_target_with_http_client_factory_blueprint_dependency_with_bad_request_response()
    {
        // Arrange
        var addAssetCommand = new AddAssetCommand("Computer");
        using var mockApiMessageHandlerBuilder = new MockApiMessageHandlerBuilder();

        // The handler is owned and disposed by the builder above.
#pragma warning disable IDISP001
        var mockApiMessageHandler = mockApiMessageHandlerBuilder.BadRequestResponse();
#pragma warning restore IDISP001
        var httpClientFactoryBlueprint = new HttpClientFactoryBlueprint(mockApiMessageHandler);

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(httpClientFactoryBlueprint)
            .Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(addAssetCommand, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task
        Should_be_able_to_create_target_with_custom_dependency_with_failing_validation_command_result()
    {
        // Arrange
        const string validationError = "Custom validation error";
        var addAssetCommand = new AddAssetCommand("Computer");
        var validateAssetCommand = Substitute.For<IValidateAssetCommandHandler>();
        validateAssetCommand.HandleAsync(new ValidateAssetCommand(addAssetCommand), TestContext.Current.CancellationToken)
            .Returns(CommandResult.Failure(validationError));
        var httpClientFactoryBlueprint = new HttpClientFactoryBlueprint();

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(httpClientFactoryBlueprint)
            .With(validateAssetCommand)
            .Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(addAssetCommand, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(validationError);
    }

    [Fact]
    public async Task Should_be_able_to_create_target_with_logger_dependency()
    {
        // Arrange
        var addAssetCommand = new AddAssetCommand("Computer");
        var validateAssetCommand = new ValidateAssetCommand(addAssetCommand);

        // Act
        var target = new TestTargetBuilder().Build<ValidateAssetCommandHandler>();
        var result = await target.HandleAsync(validateAssetCommand, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        Should_be_able_to_create_target_with_interface_that_has_inheriting_type_and_logger_dependency_with_overide_for_dependency_type_for_failure()
    {
        // Arrange
        var addPersonCommand = new AddPersonCommand("Joe Blog");
        var mockValidatePersonCommandHandler = Substitute.For<IValidatePersonCommandHandler>();
        mockValidatePersonCommandHandler.HandleAsync(Arg.Any<ValidatePersonCommand>(), TestContext.Current.CancellationToken)
            .Returns(CommandResult.Failure("Validation failed"));

        // Act
        var target = new TestTargetBuilder()
            .With(mockValidatePersonCommandHandler)
            .Build<AddPersonCommandHandler>();
        var result = await target.HandleAsync(addPersonCommand, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_be_able_to_create_target_with_interface_that_has_inheriting_type_and_logger_dependency()
    {
        // Arrange
        var addPersonCommand = new AddPersonCommand("Joe Blog");

        // Act
        var target = new TestTargetBuilder().Build<AddPersonCommandHandler>();
        var result = await target.HandleAsync(addPersonCommand, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_throw_exception_when_passing_blueprint_dependency_to_builder_that_is_null()
    {
        // Arrange
        // Act
        var target = () => new TestTargetBuilder()
            .WithBlueprint<IValidateJobCommandHandler>(null!)
            .Build<AddJobCommandHandler>();

        // Assert
        target.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Should_throw_exception_when_passing_options_to_builder_that_is_null()
    {
        // Arrange
        // Act
        var target = () => new TestTargetBuilder()
            .WithOptions<AppConfiguration>(null!)
            .Build<AddJobCommandHandler>();

        // Assert
        target.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Should_throw_exception_when_passing_dependency_to_builder_that_is_null()
    {
        // Arrange
        // Act
        var target = () => new TestTargetBuilder()
            .With<IValidateJobCommandHandler>(null!)
            .Build<AddJobCommandHandler>();

        // Assert
        target.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Should_throw_exception_when_passing_dependency_to_builder_that_has_already_being_passed_to_the_builder()
    {
        // Arrange
        var validateAssetCommand = new TestTargetBuilder().Build<ValidateAssetCommandHandler>();

        // Act
        var target = () => new TestTargetBuilder()
            .With<IValidateAssetCommandHandler>(validateAssetCommand)
            .With<IValidateAssetCommandHandler>(validateAssetCommand)
            .Build<AddPersonCommandHandler>();

        // Assert
        target.ShouldThrow<TestTargetBuilderException>();
    }

    [Fact]
    public void Should_throw_exception_when_trying_to_create_target_with_interface_but_no_inheriting_type()
    {
        // Arrange
        // Act
        var target = () => new TestTargetBuilder().Build<AddJobCommandHandler>();

        // Assert
        target.ShouldThrow<TestTargetBuilderException>();
    }

    [Fact]
    public async Task Should_apply_a_result_customisation_to_an_async_call_on_a_customised_blueprint()
    {
        // Arrange
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();
        blueprint.Customisations.Add(SendReturns(false));

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(blueprint)
            .Build<NotificationService>();
        var sent = await target.NotifyAsync("Hello");

        // Assert
        sent.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_apply_an_exception_customisation_to_an_async_call_on_a_customised_blueprint()
    {
        // Arrange
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();
        blueprint.Customisations.Add(new BlueprintCustomisation<INotificationSender, bool>(
            sender => sender.SendAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()),
            new TimeoutException()));

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(blueprint)
            .Build<NotificationService>();
        var notify = () => target.NotifyAsync("Hello");

        // Assert
        await notify.ShouldThrowAsync<TimeoutException>();
    }

    [Fact]
    public void Should_apply_a_result_customisation_to_a_synchronous_call_on_a_customised_blueprint()
    {
        // Arrange
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();
        blueprint.Customisations.Add(new BlueprintCustomisation<INotificationSender, string>(
            sender => sender.GetChannel(),
            "sms"));

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(blueprint)
            .Build<NotificationService>();

        // Assert
        target.GetChannel().ShouldBe("sms");
    }

    [Fact]
    public void Should_apply_an_exception_customisation_to_a_synchronous_call_on_a_customised_blueprint()
    {
        // Arrange
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();
        blueprint.Customisations.Add(new BlueprintCustomisation<INotificationSender, string>(
            sender => sender.GetChannel(),
            new InvalidOperationException()));

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(blueprint)
            .Build<NotificationService>();
        var getChannel = () => target.GetChannel();

        // Assert
        getChannel.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_use_the_last_customisation_added_when_two_match_the_same_call()
    {
        // Arrange
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();
        blueprint.Customisations.Add(SendReturns(true));
        blueprint.Customisations.Add(SendReturns(false));

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(blueprint)
            .Build<NotificationService>();
        var sent = await target.NotifyAsync("Hello");

        // Assert
        sent.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_only_apply_a_customisation_to_calls_matching_its_arguments()
    {
        // Arrange
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();
        blueprint.Customisations.Add(new BlueprintCustomisation<INotificationSender, bool>(
            sender => sender.SendAsync(Arg.Is<string>(message => message == "Hello"), Arg.Any<CancellationToken>()),
            true));

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(blueprint)
            .Build<NotificationService>();
        var matching = await target.NotifyAsync("Hello");
        var other = await target.NotifyAsync("Goodbye");

        // Assert
        matching.ShouldBeTrue();
        other.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_expose_the_substitute_built_by_a_customised_blueprint_so_calls_can_be_verified()
    {
        // Arrange
        var blueprint = new NotificationSenderBlueprint();
        var target = new TestTargetBuilder()
            .WithBlueprint(blueprint)
            .Build<NotificationService>();

        // Act
        await target.NotifyAsync("Hello");

        // Assert
        await blueprint.MockDependency.Received(1).SendAsync("Hello", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_build_a_working_substitute_from_a_customised_blueprint_with_no_customisations()
    {
        // Arrange
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(blueprint)
            .Build<NotificationService>();
        var sent = await target.NotifyAsync("Hello");

        // Assert
        sent.ShouldBeFalse();
    }

    [Fact]
    public void Should_have_a_mock_dependency_before_a_customised_blueprint_is_built()
    {
        // Act
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();

        // Assert
        blueprint.MockDependency.ShouldNotBeNull();
    }

    [Fact]
    public void Should_return_the_mock_dependency_every_time_a_customised_blueprint_is_built()
    {
        // Arrange
        var blueprint = new NotificationSenderBlueprint();

        // Act
        var first = blueprint.Build();
        var second = blueprint.Build();

        // Assert
        first.ShouldBeSameAs(blueprint.MockDependency);
        second.ShouldBeSameAs(first);
    }

    [Fact]
    public void Should_apply_customisations_to_a_mock_dependency_that_has_been_set()
    {
        // Arrange
        var substitute = Substitute.For<INotificationSender>();
        var blueprint = new NotificationSenderBlueprint { MockDependency = substitute };

        // Act
        var built = blueprint.Build();

        // Assert
        built.ShouldBeSameAs(substitute);
        built.GetChannel().ShouldBe(NotificationSenderBlueprint.DefaultChannel);
    }

    [Fact]
    public async Task Should_apply_customisations_added_after_a_customised_blueprint_was_first_built()
    {
        // Arrange
        var blueprint = new CustomisedBlueprintDependency<INotificationSender>();
        blueprint.Build();
        blueprint.Customisations.Add(SendReturns(true));

        // Act
        var built = blueprint.Build();
        var sent = await built.SendAsync("Hello", CancellationToken.None);

        // Assert
        sent.ShouldBeTrue();
    }

    [Fact]
    public void Should_throw_exception_when_applying_a_customisation_to_a_null_substitute()
    {
        // Arrange
        var customisation = SendReturns(true);

        // Act
        var apply = () => customisation.Apply(null!);

        // Assert
        apply.ShouldThrow<ArgumentNullException>();
    }

    private static BlueprintCustomisation<INotificationSender, bool> SendReturns(bool result)
    {
        return new BlueprintCustomisation<INotificationSender, bool>(
            sender => sender.SendAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()),
            result);
    }
}

using System.Net;
using System.Text.Json;
using Audacia.Commands;
using Audacia.UnitTest.Dependency.Exceptions;
using Audacia.UnitTest.Dependency.Http;
using Audacia.UnitTest.Dependency.Http.Blueprints;
using Audacia.UnitTest.Dependency.Http.Builders;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Configuration;
using NSubstitute;
using RichardSzalay.MockHttp;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

public class TestTargetBuilderTests
{
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
        var result = await target.HandleAsync(addAssetCommand);

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
        var mockApiMessageHandler = new MockApiMessageHandlerBuilder().BadRequestResponse();
        var httpClientFactoryBlueprint = new HttpClientFactoryBlueprint(mockApiMessageHandler);

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(httpClientFactoryBlueprint)
            .Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(addAssetCommand);

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
        validateAssetCommand.HandleAsync(new ValidateAssetCommand(addAssetCommand))
            .Returns(CommandResult.Failure(validationError));
        var httpClientFactoryBlueprint = new HttpClientFactoryBlueprint();

        // Act
        var target = new TestTargetBuilder()
            .WithBlueprint(httpClientFactoryBlueprint)
            .With(validateAssetCommand)
            .Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(addAssetCommand);

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
        var result = await target.HandleAsync(validateAssetCommand);

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
        mockValidatePersonCommandHandler.HandleAsync(Arg.Any<ValidatePersonCommand>())
            .Returns(CommandResult.Failure("Validation failed"));

        // Act
        var target = new TestTargetBuilder()
            .With(mockValidatePersonCommandHandler)
            .Build<AddPersonCommandHandler>();
        var result = await target.HandleAsync(addPersonCommand);

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
        var result = await target.HandleAsync(addPersonCommand);

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
}
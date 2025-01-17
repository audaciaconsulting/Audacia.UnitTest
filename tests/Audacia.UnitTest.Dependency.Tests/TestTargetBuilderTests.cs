using Audacia.UnitTest.Dependency.Exceptions;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Audacia.UnitTest.Dependency.Tests;

public class TestTargetBuilderTests
{
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
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
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
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Should_throw_exception_when_trying_to_create_target_with_interface_but_no_inheriting_type()
    {
        // Arrange
        var addJobCommand = new AddJobCommand("J1001");

        // Act
        var target = new TestTargetBuilder().Build<AddJobCommandHandler>();
        var result = () => target.HandleAsync(addJobCommand);

        // Assert
        await result.Should().ThrowAsync<TestTargetBuilderException>();
    }

    // [Fact]
    // public async Task Should_be_able_to_create_with_all_dependencies_provided_with_passed_in_blueprint_dependency()
    // {
    //     // Arrange
    //     var expectedMessage = MockCommandBlueprint.DefaultResponseMessage;
    //
    //     var mockCommandHandler = Substitute.For<IMockCommandHandler>();
    //     var mockedResult = CommandResult.WithResult(new MockCommandResultDto(expectedMessage, null));
    //     mockCommandHandler.HandleAsync(Arg.Any<MockCommand>(), Arg.Any<CancellationToken>()).Returns(mockedResult);
    //
    //     var mockCommandBlueprintDependency = new MockCommandBlueprint(mockCommandHandler);
    //
    //     // Act
    //     var target = new TestTargetBuilder()
    //         .WithBlueprint(mockCommandBlueprintDependency)
    //         .Build<AddAssetCommandHandler>();
    //     var result = await target.HandleAsync(new AddAssetCommand());
    //
    //     // Assert
    //     using (new AssertionScope())
    //     {
    //         result.Should().NotBeNull();
    //         result.IsSuccess.Should().BeTrue();
    //         result.Errors.Should().BeEmpty();
    //         result.Output.ResultOne.Should().Be(
    //             expectedMessage,
    //             "it is the default message configured for the blueprint.");
    //         result.Output.ResultTwo.Should().BeNull("has no value from the mock handler instance");
    //     }
    // }
    //
    // [Fact]
    // public async Task Should_throw_exception_when_blueprint_dependency_is_present_but_has_no_setup_for_mock()
    // {
    //     // Act
    //     var target = new TestTargetBuilder().Build<AddAssetCommandHandler>();
    //     var result = () => target.HandleAsync(new AddAssetCommand());
    //
    //     // Assert
    //     await result.Should().ThrowAsync<NullReferenceException>();
    // }
    //
    // [Fact]
    // public async Task Should_be_able_to_create_with_all_dependencies_provided_but_with_provided_customisation()
    // {
    //     // Arrange
    //     const string expectedReturnMessageOne = "Overriden message one";
    //     const string expectedReturnMessageTwo = "Overriden message two";
    //     var mockResult = new MockCommandResultDto(expectedReturnMessageOne, expectedReturnMessageTwo);
    //
    //     // Act
    //     var target = new TestTargetBuilder()
    //         .Build<AddAssetCommandHandler>();
    //     var result = await target.HandleAsync(new AddAssetCommand());
    //
    //     // Assert
    //     using (new AssertionScope())
    //     {
    //         result.Should().NotBeNull();
    //         result.IsSuccess.Should().BeTrue();
    //         result.Errors.Should().BeEmpty();
    //         result.Output.ResultOne.Should().Be(
    //             expectedReturnMessageOne,
    //             "the mock should have overriden the return message one");
    //         result.Output.ResultTwo.Should().Be(
    //             expectedReturnMessageTwo,
    //             "the mock should have overriden the return message two");
    //     }
    // }
    //
    // [Fact]
    // public async Task
    //     Should_be_able_to_create_with_all_dependencies_provided_but_with_multiple_provided_customisation_to_use_last_registered_that_matches()
    // {
    //     // Arrange
    //     const string expectedErrorMessage = "Example error message";
    //     const string expectedReturnMessageOne = "Overriden message one";
    //     const string expectedReturnMessageTwo = "Overriden message two";
    //     var mockResult = new MockCommandResultDto(expectedReturnMessageOne, expectedReturnMessageTwo);
    //
    //     var httpClientBlueprint = new HttpClientBlueprint();
    //
    //     httpClientBlueprint.Customise(
    //         mockApiMessageHandler =>
    //             mockApiMessageHandler.When("https://localhost:11111").Respond(
    //                 HttpStatusCode.OK,
    //                 "application/json",
    //                 JsonSerializer.Serialize(
    //                     new ExternalApiResponse()
    //                     {
    //                         StatusCode = 200,
    //                         IsSuccess = true,
    //                         ReceivedAt = DateTime.Now,
    //                         ErrorDetails = null,
    //                         Message = "Success"
    //                     })));
    //
    //     // Act
    //     var target = new TestTargetBuilder()
    //         .With(httpClientBlueprint)
    //         .Build<AddAssetCommandHandler>();
    //     var result = await target.HandleAsync(new AddAssetCommand());
    //
    //     // Assert
    //     using (new AssertionScope())
    //     {
    //         result.Should().NotBeNull();
    //         result.IsSuccess.Should().BeFalse();
    //         result.Errors.Should().Contain(expectedErrorMessage);
    //         result.Output.Should().BeNull();
    //     }
    // }
}
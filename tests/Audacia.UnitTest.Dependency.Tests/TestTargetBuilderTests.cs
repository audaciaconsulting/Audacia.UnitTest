using Audacia.Commands;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Blueprints;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Stud;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

namespace Audacia.UnitTest.Dependency.Tests;

public class TestTargetBuilderTests
{
    [Fact]
    public async Task Should_be_able_to_create_with_all_dependencies_provided_with_passed_in_dependency_instance()
    {
        // Arrange
        var expectedMessage = MockCommandBlueprintDependency.DefaultResponseMessage;
        var mockCommandHandler = new Mock<IMockCommandHandler>();

        mockCommandHandler.Setup(command => command.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandResult.WithResult(new MockCommandResultDto(expectedMessage, null)));

        // Act
        var target = new TestTargetBuilder().With(mockCommandHandler.Object).Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(new AddAssetCommand());

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Output.ResultOne.Should().Be(
                expectedMessage,
                "it is the default message configured for the blueprint.");
            result.Output.ResultTwo.Should().BeNull("has no value from the mock handler instance");
        }
    }

    [Fact]
    public async Task Should_be_able_to_create_with_all_dependencies_provided_with_passed_in_blueprint_dependency()
    {
        // Arrange
        var expectedMessage = MockCommandBlueprintDependency.DefaultResponseMessage;
        var mockCommandHandler = new Mock<IMockCommandHandler>();

        mockCommandHandler.Setup(command => command.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandResult.WithResult(new MockCommandResultDto(expectedMessage, null)));

        var mockCommandBlueprintDependency = new MockCommandBlueprintDependency(mockCommandHandler);

        // Act
        var target = new TestTargetBuilder().WithDependency(mockCommandBlueprintDependency)
            .Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(new AddAssetCommand());

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Output.ResultOne.Should().Be(
                expectedMessage,
                "it is the default message configured for the blueprint.");
            result.Output.ResultTwo.Should().BeNull("has no value from the mock handler instance");
        }
    }

    [Fact]
    public async Task Should_throw_exception_when_blueprint_dependency_is_present_but_has_no_setup_for_mock()
    {
        // Act
        var target = new TestTargetBuilder().Build<AddAssetCommandHandler>();
        var result = () => target.HandleAsync(new AddAssetCommand());

        // Assert
        await result.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task Should_be_able_to_create_with_all_dependencies_provided_but_with_provided_customisation()
    {
        // Arrange
        const string expectedReturnMessageOne = "Overriden message one";
        const string expectedReturnMessageTwo = "Overriden message two";
        var mockResult = new MockCommandResultDto(expectedReturnMessageOne, expectedReturnMessageTwo);

        // Act
        var target = new TestTargetBuilder()
            .Customise<IMockCommandHandler, Task<CommandResult<MockCommandResultDto>>>(
                mockCommand => mockCommand.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()),
                Task.FromResult(CommandResult.WithResult(mockResult)))
            .Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(new AddAssetCommand());

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Output.ResultOne.Should().Be(
                expectedReturnMessageOne,
                "the mock should have overriden the return message one");
            result.Output.ResultTwo.Should().Be(
                expectedReturnMessageTwo,
                "the mock should have overriden the return message two");
        }
    }

    [Fact]
    public async Task
        Should_be_able_to_create_with_all_dependencies_provided_but_with_multiple_provided_customisation_to_use_last_registered_that_matches()
    {
        // Arrange
        const string expectedErrorMessage = "Example error message";
        const string expectedReturnMessageOne = "Overriden message one";
        const string expectedReturnMessageTwo = "Overriden message two";
        var mockResult = new MockCommandResultDto(expectedReturnMessageOne, expectedReturnMessageTwo);

        // Act
        var target = new TestTargetBuilder()
            .Customise<IMockCommandHandler, Task<CommandResult<MockCommandResultDto>>>(
                mockCommand => mockCommand.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()),
                Task.FromResult(CommandResult.WithResult(mockResult)))
            .Customise<IMockCommandHandler, Task<CommandResult<MockCommandResultDto>>>(
                mockCommand => mockCommand.HandleAsync(It.IsAny<MockCommand>(), It.IsAny<CancellationToken>()),
                Task.FromResult(CommandResult.Failure<MockCommandResultDto>(expectedErrorMessage)))
            .Build<AddAssetCommandHandler>();
        var result = await target.HandleAsync(new AddAssetCommand());

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(expectedErrorMessage);
            result.Output.Should().BeNull();
        }
    }
}
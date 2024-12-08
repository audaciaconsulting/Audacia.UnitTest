using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Stud;

public class MockCommandHandler : IMockCommandHandler
{
    public Task<CommandResult<MockCommandResultDto>> HandleAsync(
        MockCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new MockCommandResultDto("I like to cook", null);
        return Task.FromResult(CommandResult.WithResult(result));
    }
}
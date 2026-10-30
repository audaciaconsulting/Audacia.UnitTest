using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Validate;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;

/// <summary>
/// This is used when setting up the handler 'manually', for examples that don't use Audacia.UnitTest.Dependency.Mediator.
/// </summary>
/// <param name="logger"></param>
/// <param name="validateJobCommandHandler"></param>
public class AddJobCommandHandler(
    ILogger<AddJobCommandHandler> logger,
    IValidateJobCommandHandler validateJobCommandHandler) : IAddJobCommandHandler
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResult<AddJobCommandResultDto>> HandleAsync(
        AddJobCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entry: Adding job with name: {@NewJobName}", command.Name);

        var validateJobCommand = new ValidateJobCommand(command);
        var validationResult = await validateJobCommandHandler.HandleAsync(validateJobCommand, cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return CommandResult.Failure<AddJobCommandResultDto>(validationResult.Errors);
        }

        var result = new AddJobCommandResultDto(validationResult);
        return CommandResult.WithResult(result);
    }
}
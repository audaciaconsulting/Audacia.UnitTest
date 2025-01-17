using Audacia.Commands;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;

/// <summary>
///
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
        logger.LogInformation("Entry: Adding person with name: {@NewAssetName}", command.Name);

        var validateJobCommand = new ValidateJobCommand(command);
        var validationResult = await validateJobCommandHandler.HandleAsync(validateJobCommand, cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return CommandResult.FromExistingResult<AddJobCommandResultDto>(validationResult);
        }

        var result = new AddJobCommandResultDto(validationResult);
        return CommandResult.WithResult(result);
    }
}
using System.Text;
using System.Text.Json;
using Audacia.Commands;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;

/// <summary>
///
/// </summary>
/// <param name="logger"></param>
/// <param name="validatePersonCommandHandler"></param>
public class AddPersonCommandHandler(
    ILogger<AddPersonCommandHandler> logger,
    IValidatePersonCommandHandler validatePersonCommandHandler) : IAddPersonCommandHandler
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResult<AddPersonCommandResultDto>> HandleAsync(
        AddPersonCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entry: Adding person with name: {@NewAssetName}", command.Name);

        var validatePersonCommand = new ValidatePersonCommand(command);
        var validationResult = await validatePersonCommandHandler.HandleAsync(validatePersonCommand, cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return CommandResult.FromExistingResult<AddPersonCommandResultDto>(validationResult);
        }

        var result = new AddPersonCommandResultDto(validationResult);
        return CommandResult.WithResult(result);
    }
}
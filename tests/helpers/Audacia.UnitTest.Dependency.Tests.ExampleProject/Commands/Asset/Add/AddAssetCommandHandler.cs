using Audacia.Commands;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Stud;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

public class AddAssetCommandHandler(
    ILogger<AddAssetCommandHandler> logger,
    IValidateAssetCommand validateAssetCommand,
    IMockCommandHandler mockCommandHandler) : IAddAssetCommandHandler
{
    public async Task<CommandResult<AddAssetCommandResultDto>> HandleAsync(
        AddAssetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entry: Adding asset with name: {@NewAssetName}", command.Name);

        var mockCommandResult = await mockCommandHandler.HandleAsync(new MockCommand(), cancellationToken);

        if (!mockCommandResult.IsSuccess)
        {
            return CommandResult.Failure<AddAssetCommandResultDto>(mockCommandResult.Errors.ToArray());
        }

        var result = new AddAssetCommandResultDto(
            mockCommandResult.Output.ResultOne,
            mockCommandResult.Output.ResultTwo);
        return CommandResult.WithResult(result);
    }
}
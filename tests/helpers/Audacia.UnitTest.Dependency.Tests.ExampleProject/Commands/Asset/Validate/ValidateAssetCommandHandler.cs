using Audacia.Mediator;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;

/// <summary>
/// Validates an asset that has been requested to be added or updated.
/// </summary>
/// <param name="logger"></param>
public class ValidateAssetCommandHandler(ILogger<ValidateAssetCommandHandler> logger)
    : IRequestHandler<ValidateAssetCommand, CommandResult>
{
    /// <summary>
    /// Validate an asset that has been requested to be added or updated.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<CommandResult> HandleAsync(
        ValidateAssetCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        logger.LogInformation("Entry: Validating asset with Name: {@ValidateAssetName}", command.Asset.Name);

        if (command.Asset.Name.Length > 10)
        {
            logger.LogWarning("Exit: Asset name is too long");
            return Task.FromResult(CommandResult.Failure("Asset name cannot be longer than 10 characters."));
        }

        logger.LogInformation("Exit: Asset is valid");
        return Task.FromResult(CommandResult.Success());
    }
}
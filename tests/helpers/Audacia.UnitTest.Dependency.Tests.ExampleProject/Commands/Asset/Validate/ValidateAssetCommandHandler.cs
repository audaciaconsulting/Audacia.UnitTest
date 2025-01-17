using Audacia.Commands;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;

/// <summary>
/// Validates an asset that has being request to be added or updated.
/// </summary>
/// <param name="logger"></param>
public class ValidateAssetCommandHandler(ILogger<ValidateAssetCommandHandler> logger) : IValidateAssetCommandHandler
{
    /// <summary>
    /// Validate an asset that has being requested to be added or updated.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResult> HandleAsync(
        ValidateAssetCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Asset.Name.Length > 10)
        {
            return CommandResult.Failure("Asset name cannot be longer than 10 characters.");
        }

        return CommandResult.Success();
    }
}
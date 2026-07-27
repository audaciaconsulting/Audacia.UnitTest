using System.Text;
using System.Text.Json;
using Audacia.Commands;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

/// <summary>
///
/// </summary>
/// <param name="logger"></param>
/// <param name="validateAssetCommandHandler"></param>
/// <param name="httpClientFactory"></param>
public class AddAssetCommandHandler(
    ILogger<AddAssetCommandHandler> logger,
    IValidateAssetCommandHandler validateAssetCommandHandler,
    IHttpClientFactory httpClientFactory) : IAddAssetCommandHandler
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResult<AddAssetCommandResultDto>> HandleAsync(
        AddAssetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entry: Adding asset with name: {@NewAssetName}", command.Name);

        var validateAssetCommand = new ValidateAssetCommand(command);
        var validationResult = await validateAssetCommandHandler.HandleAsync(validateAssetCommand, cancellationToken);

        return !validationResult.IsSuccess
            ? CommandResult.FromExistingResult<AddAssetCommandResultDto>(validationResult)
            : await AddAssetAsync(command, validationResult, cancellationToken);
    }

    private async Task<CommandResult<AddAssetCommandResultDto>> AddAssetAsync(
        AddAssetCommand command,
        CommandResult validationResult,
        CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient();

        var json = JsonSerializer.Serialize(command);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = new Uri("https://localhost:11111");
        using var response = await httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return CommandResult.Failure<AddAssetCommandResultDto>(
                $"None successful code returned: {response.StatusCode}");
        }

        var result = new AddAssetCommandResultDto(validationResult, response.StatusCode);
        return CommandResult.WithResult(result);
    }
}
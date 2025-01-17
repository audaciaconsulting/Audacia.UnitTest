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

        if (!validationResult.IsSuccess)
        {
            return CommandResult.FromExistingResult<AddAssetCommandResultDto>(validationResult);
        }

        using var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri("https://localhost:111111");

        var json = JsonSerializer.Serialize(command);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync(new Uri(string.Empty), content, cancellationToken);

        var result = new AddAssetCommandResultDto(validationResult, response.StatusCode);
        return CommandResult.WithResult(result);
    }
}
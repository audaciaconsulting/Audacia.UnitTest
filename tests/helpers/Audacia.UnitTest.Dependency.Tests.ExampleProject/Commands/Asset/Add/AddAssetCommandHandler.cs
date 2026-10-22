using System.Text;
using System.Text.Json;
using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

/// <summary>
///
/// </summary>
/// <param name="logger"></param>
/// <param name="mediator"></param>
/// <param name="httpClientFactory"></param>
public class AddAssetCommandHandler(
    ILogger<AddAssetCommandHandler> logger,
    IMediator mediator,
    IHttpClientFactory httpClientFactory) : IRequestHandler<AddAssetCommand, CommandResult<AddAssetCommandResultDto>>
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

        var validationResult = await mediator.SendAsync(new ValidateAssetCommand(command), cancellationToken);

        return !validationResult.IsSuccess
            ? CommandResult.Failure<AddAssetCommandResultDto>(validationResult.Errors)
            : await AddAssetAsync(command, cancellationToken);
    }

    private async Task<CommandResult<AddAssetCommandResultDto>> AddAssetAsync(
        AddAssetCommand command,
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

        var result = new AddAssetCommandResultDto(command.Name, response.StatusCode);
        return CommandResult.WithResult(result);
    }
}
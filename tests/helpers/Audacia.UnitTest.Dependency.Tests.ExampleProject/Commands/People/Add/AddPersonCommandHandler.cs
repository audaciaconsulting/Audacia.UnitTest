using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;

/// <summary>
///
/// </summary>
/// <param name="logger"></param>
/// <param name="mediator"></param>
public class AddPersonCommandHandler(
    ILogger<AddPersonCommandHandler> logger,
    IMediator mediator) : IRequestHandler<AddPersonCommand, CommandResult<AddPersonCommandResultDto>>
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
        logger.LogInformation("Entry: Adding person with name: {@NewPersonName}", command.Name);

        var validationResult = await mediator.SendAsync(new ValidatePersonCommand(command), cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return CommandResult.Failure<AddPersonCommandResultDto>(validationResult.Errors);
        }

        var result = new AddPersonCommandResultDto(command.Name);
        return CommandResult.WithResult(result);
    }
}
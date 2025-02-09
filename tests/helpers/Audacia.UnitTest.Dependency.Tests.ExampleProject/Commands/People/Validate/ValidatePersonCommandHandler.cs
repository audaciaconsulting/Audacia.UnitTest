using Audacia.Commands;
using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;

/// <summary>
/// Validates a person that has being request to be added or updated.
/// </summary>
/// <param name="logger"></param>
public class ValidatePersonCommandHandler(ILogger<ValidatePersonCommandHandler> logger) : IValidatePersonCommandHandler
{
    /// <summary>
    /// Validate a person that has being requested to be added or updated.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<CommandResult> HandleAsync(
        ValidatePersonCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        logger.LogInformation("Entry: Validating person with Name: {@ValidatePersonName}", command.Person.Name);

        if (command.Person.Name.Length > 25)
        {
            logger.LogWarning("Exit: Person name is too long");
            return Task.FromResult(CommandResult.Failure("Person name cannot be longer than 25 characters."));
        }

        logger.LogInformation("Exit: Person is valid");
        return Task.FromResult(CommandResult.Success());
    }
}
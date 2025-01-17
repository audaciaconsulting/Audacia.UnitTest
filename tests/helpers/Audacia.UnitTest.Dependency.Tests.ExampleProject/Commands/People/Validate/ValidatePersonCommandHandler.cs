using Audacia.Commands;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;
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
    public async Task<CommandResult> HandleAsync(
        ValidatePersonCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Person.Name.Length > 25)
        {
            return CommandResult.Failure("Person name cannot be longer than 25 characters.");
        }

        return CommandResult.Success();
    }
}
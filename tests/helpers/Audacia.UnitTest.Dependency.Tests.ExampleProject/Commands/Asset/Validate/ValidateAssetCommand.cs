using Microsoft.Extensions.Logging;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;

public class ValidateAssetCommand : IValidateAssetCommand
{
    public ValidateAssetCommand(ILogger<ValidateAssetCommand> logger)
    {
    }
}
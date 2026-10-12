using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;

/// <summary>
/// Interface for validating an asset.
/// </summary>
public interface IValidateAssetCommandHandler : IRequestHandler<ValidateAssetCommand, CommandResult>;
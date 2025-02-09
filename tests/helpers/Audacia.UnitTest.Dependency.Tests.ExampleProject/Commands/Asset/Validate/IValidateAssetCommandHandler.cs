using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;

/// <summary>
/// Interface for validating an asset.
/// </summary>
public interface IValidateAssetCommandHandler : ICommandHandler<ValidateAssetCommand>;
using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

/// <summary>
/// Interface for adding a new asset.
/// </summary>
public interface IAddAssetCommandHandler : ICommandHandler<AddAssetCommand, AddAssetCommandResultDto>;
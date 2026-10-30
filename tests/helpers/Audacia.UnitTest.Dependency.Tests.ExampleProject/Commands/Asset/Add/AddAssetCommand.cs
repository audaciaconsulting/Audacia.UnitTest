using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

/// <summary>
///
/// </summary>
/// <param name="Name"></param>
public record AddAssetCommand(string Name) : IRequest<CommandResult<AddAssetCommandResultDto>>;
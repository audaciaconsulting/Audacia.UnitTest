using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

public interface IAddAssetCommandHandler : ICommandHandler<AddAssetCommand, AddAssetCommandResultDto>
{
}
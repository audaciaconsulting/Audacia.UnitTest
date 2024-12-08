using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

public class AddAssetCommand : ICommand
{
    public string Name { get; set; }
}
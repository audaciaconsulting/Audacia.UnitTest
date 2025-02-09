using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

/// <summary>
///
/// </summary>
/// <param name="Name"></param>
public record AddAssetCommand(string Name) : ICommand;
using Audacia.Commands;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Validate;

/// <summary>
///
/// </summary>
/// <param name="Asset"></param>
public record ValidateAssetCommand(AddAssetCommand Asset) : ICommand;
using System.Net;
using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

/// <summary>
///
/// </summary>
/// <param name="ValidateAssetResult"></param>
/// <param name="AddAssetResultCode"></param>
public record AddAssetCommandResultDto(
    CommandResult ValidateAssetResult,
    HttpStatusCode AddAssetResultCode);
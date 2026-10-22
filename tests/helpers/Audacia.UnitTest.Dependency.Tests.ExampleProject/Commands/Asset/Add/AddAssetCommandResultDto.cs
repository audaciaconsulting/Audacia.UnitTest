using System.Net;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;

/// <summary>
///
/// </summary>
/// <param name="Name"></param>
/// <param name="AddAssetResultCode"></param>
public record AddAssetCommandResultDto(
    string Name,
    HttpStatusCode AddAssetResultCode);
using Audacia.Azure.BlobStorage.AddBlob;
using Audacia.Azure.BlobStorage.AddBlob.Commands;
using Audacia.UnitTest.Dependency.Customisations;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.Storage;

/// <summary>
/// Blueprint for a dependency of <see cref="IAddAzureBlobStorageService"/>.
/// </summary>
public class AddAzureBlobStorageServiceBlueprint : CustomisedBlueprintDependency<IAddAzureBlobStorageService>
{
    /// <summary>
    /// Creates a default <see cref="AddAzureBlobStorageServiceBlueprint"/> where any blob added will be accepted.
    /// </summary>
    public AddAzureBlobStorageServiceBlueprint()
    {
        const bool defaultResponse = true;

        var happyPathCustomisations = new BlueprintCustomisation<IAddAzureBlobStorageService, bool>(
            addBlobCommand => addBlobCommand.ExecuteAsync(
                Arg.Any<AddBlobBytesCommand>(),
                Arg.Any<CancellationToken>()),
                defaultResponse);

        Customisations.Add(happyPathCustomisations);
    }
}

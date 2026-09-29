using Audacia.Azure.BlobStorage.DeleteBlob;
using Audacia.Azure.BlobStorage.DeleteBlob.Commands;
using Audacia.UnitTest.Dependency.Customisations;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.Storage;

/// <summary>
/// Blueprint for a dependency of <see cref="IDeleteAzureBlobStorageService"/>.
/// </summary>
public class DeleteAzureBlobStorageServiceBlueprint : CustomisedBlueprintDependency<IDeleteAzureBlobStorageService>
{
    /// <summary>
    /// Creates a default <see cref="DeleteAzureBlobStorageServiceBlueprint"/> where any blob deleted will be accepted.
    /// </summary>
    public DeleteAzureBlobStorageServiceBlueprint()
    {
        const bool defaultResponse = true;
        var happyPathCustomisations = new BlueprintCustomisation<IDeleteAzureBlobStorageService, bool>(
            redactBlobCommand => redactBlobCommand.ExecuteAsync(
                Arg.Any<DeleteAzureBlobStorageCommand>(),
                Arg.Any<CancellationToken>()),
            defaultResponse);

        Customisations.Add(happyPathCustomisations);
    }
}
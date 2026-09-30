using Audacia.Azure.BlobStorage.DeleteBlob;
using Audacia.Azure.BlobStorage.DeleteBlob.Commands;
using Audacia.UnitTest.Dependency.Customisations;
using Azure;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.Storage;

/// <summary>
/// A blueprint for an <see cref="IDeleteAzureBlobStorageService"/>.
/// This is the object for deleting blobs from blob storage.
/// </summary>
public sealed class DeleteAzureBlobStorageServiceBlueprint : CustomisedBlueprintDependency<IDeleteAzureBlobStorageService>
{
    private const int ServiceUnavailableStatus = 503;

    /// <summary>
    /// Creates a default <see cref="DeleteAzureBlobStorageServiceBlueprint"/> where any blob deleted will be accepted.
    /// </summary>
    public DeleteAzureBlobStorageServiceBlueprint()
    {
        const bool defaultResponse = true;

        Customisations.Add(new BlueprintCustomisation<IDeleteAzureBlobStorageService, bool>(
            DeleteCall(),
            defaultResponse));
    }

    /// <summary>
    /// Creates a <see cref="DeleteAzureBlobStorageServiceBlueprint"/> where any blob deleted will throw an exception.
    /// </summary>
    /// <returns>Blueprint configured to throw a <see cref="RequestFailedException"/> when deleting a blob.</returns>
    public static DeleteAzureBlobStorageServiceBlueprint ThrowExceptionWhenDeleting()
    {
        var blueprint = new DeleteAzureBlobStorageServiceBlueprint();

        blueprint.Customisations.Add(new BlueprintCustomisation<IDeleteAzureBlobStorageService, bool>(
            DeleteCall(),
            new RequestFailedException(ServiceUnavailableStatus, "Blob storage is unavailable.")));

        return blueprint;
    }

    private static Func<IDeleteAzureBlobStorageService, Task<bool>> DeleteCall()
    {
        return service => service.ExecuteAsync(Arg.Any<DeleteAzureBlobStorageCommand>(), Arg.Any<CancellationToken>());
    }
}

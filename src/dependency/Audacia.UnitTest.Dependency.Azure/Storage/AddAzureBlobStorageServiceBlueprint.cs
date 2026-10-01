using Audacia.Azure.BlobStorage.AddBlob;
using Audacia.Azure.BlobStorage.AddBlob.Commands;
using Audacia.UnitTest.Dependency.Blueprints;
using Azure;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure.Storage;

/// <summary>
/// A blueprint for an <see cref="IAddAzureBlobStorageService"/>.
/// This is the object for adding blobs to blob storage, from bytes, base 64, a file or a stream.
/// </summary>
public sealed class AddAzureBlobStorageServiceBlueprint : BlueprintDependency<IAddAzureBlobStorageService>
{
    private const int ServiceUnavailableStatus = 503;

    /// <summary>
    /// Creates a default <see cref="AddAzureBlobStorageServiceBlueprint"/> where any blob added will be accepted.
    /// </summary>
    public AddAzureBlobStorageServiceBlueprint()
    {
        const bool defaultResponse = true;

        foreach (var addCall in AddCalls())
        {
            Customisations.Add(new BlueprintCustomisation<IAddAzureBlobStorageService, bool>(addCall, defaultResponse));
        }
    }

    /// <summary>
    /// Creates an <see cref="AddAzureBlobStorageServiceBlueprint"/> where any blob added will throw an exception.
    /// </summary>
    /// <returns>Blueprint configured to throw a <see cref="RequestFailedException"/> when adding a blob.</returns>
    public static AddAzureBlobStorageServiceBlueprint ThrowExceptionWhenAdding()
    {
        var blueprint = new AddAzureBlobStorageServiceBlueprint();

        foreach (var addCall in AddCalls())
        {
            blueprint.Customisations.Add(
                new BlueprintCustomisation<IAddAzureBlobStorageService, bool>(
                    addCall,
                    new RequestFailedException(ServiceUnavailableStatus, "Blob storage is unavailable.")));
        }

        return blueprint;
    }

    /// <summary>
    /// Every overload of <see cref="IAddAzureBlobStorageService"/> for adding a blob, so behaviour applies however it is called.
    /// </summary>
    private static Func<IAddAzureBlobStorageService, Task<bool>>[] AddCalls()
    {
        return
        [
            service => service.ExecuteAsync(Arg.Any<AddBlobBytesCommand>(), Arg.Any<CancellationToken>()),
            service => service.ExecuteAsync(Arg.Any<AddBlobBaseSixtyFourCommand>(), Arg.Any<CancellationToken>()),
            service => service.ExecuteAsync(Arg.Any<AddBlobFileCommand>(), Arg.Any<CancellationToken>()),
            service => service.ExecuteAsync(Arg.Any<AddBlobStreamCommand>(), Arg.Any<CancellationToken>())
        ];
    }
}

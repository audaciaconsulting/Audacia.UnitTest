using Audacia.UnitTest.Dependency.Blueprints;
using Microsoft.Extensions.Azure;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Azure;

/// <summary>
/// Blueprint for a dependency of <see cref="IAzureClientFactory{TClient}"/>.
/// </summary>
/// <typeparam name="TClient">The type of the client.</typeparam>
public abstract class AzureClientFactoryBlueprint<TClient>
    : BlueprintDependency<IAzureClientFactory<TClient>> where TClient : class
{
    /// <summary>
    /// Registers a customisation for a client instance for the given name.
    /// (the same name you would use when calling CreateClient(name)).
    /// </summary>
    /// <param name="name">Name of the client.</param>
    /// <param name="instance">Instance of the client.</param>
    protected void SetupNamedClient(
        string name,
        TClient instance)
    {
        var namedClientCustomisation = new BlueprintCustomisation<IAzureClientFactory<TClient>, TClient>(
            azureClientFactory => azureClientFactory.CreateClient(Arg.Is<string>(match => match == name)),
            instance);

        Customisations.Add(namedClientCustomisation);
    }

    /// <summary>
    /// Registers a customisation for a client instance for any name.
    /// </summary>
    /// <param name="instance">Instance of the client.</param>
    protected void SetupAnyNamedClient(TClient instance)
    {
        var namedClientCustomisation = new BlueprintCustomisation<IAzureClientFactory<TClient>, TClient>(
            azureClientFactory => azureClientFactory.CreateClient(Arg.Is<string>(_ => true)),
            instance);

        Customisations.Add(namedClientCustomisation);
    }

    /// <summary>
    /// Registers a customisation for a client instance for the given names.
    /// (the same name you would use when calling CreateClient(name)).
    /// </summary>
    /// <param name="names">Names of the clients.</param>
    /// <param name="instance">Instance of the client.</param>
    protected void SetupNamedClients(
        string[] names,
        TClient instance)
    {
        var namedClientCustomisation = new BlueprintCustomisation<IAzureClientFactory<TClient>, TClient>(
            azureClientFactory => azureClientFactory.CreateClient(Arg.Is<string>(match => names.Contains(match))),
            instance);

        Customisations.Add(namedClientCustomisation);
    }
}
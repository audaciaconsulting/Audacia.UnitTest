using Microsoft.Extensions.Options;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Configuration;

/// <summary>
/// Consumes named configuration via <see cref="IOptionsSnapshot{TOptions}"/>.
/// </summary>
/// <remarks>
/// <para>Initializes a new instance of the <see cref="AppConfigurationSnapshotConsumer"/> class.</para>
/// </remarks>
/// <param name="options">The configuration snapshot.</param>
public sealed class AppConfigurationSnapshotConsumer(IOptionsSnapshot<AppConfiguration> options)
{
    /// <summary>
    /// Gets the configuration used when no name is requested.
    /// </summary>
    public AppConfiguration Default => options.Value;

    /// <summary>
    /// Gets the configuration registered under the given name.
    /// </summary>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>The named configuration.</returns>
    public AppConfiguration Get(string name)
    {
        return options.Get(name);
    }
}

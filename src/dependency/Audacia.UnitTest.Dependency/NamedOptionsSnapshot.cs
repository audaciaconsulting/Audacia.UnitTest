using Microsoft.Extensions.Options;

namespace Audacia.UnitTest.Dependency;

/// <summary>
/// An <see cref="IOptionsSnapshot{TOptions}"/> backed by a fixed set of named configurations.
/// </summary>
/// <typeparam name="TOptions">The type of the options class.</typeparam>
/// <remarks>
/// <para>Initializes a new instance of the <see cref="NamedOptionsSnapshot{TOptions}"/> class.</para>
/// </remarks>
/// <param name="namedOptions">The configurations, keyed by name.</param>
/// <param name="defaultOptions">The configuration returned by <see cref="Value"/>.</param>
internal sealed class NamedOptionsSnapshot<TOptions>(
    IReadOnlyDictionary<string, TOptions> namedOptions,
    TOptions defaultOptions) : IOptionsSnapshot<TOptions>
    where TOptions : class
{
    /// <summary>
    /// Gets the options used when no name is requested.
    /// </summary>
    public TOptions Value { get; } = defaultOptions;

    /// <summary>
    /// Gets the configuration registered under <paramref name="name"/>,
    /// falling back to <see cref="Value"/> when the name is not registered.
    /// </summary>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>The named configuration.</returns>
    public TOptions Get(string? name)
    {
        return name is not null && namedOptions.TryGetValue(name, out var options) ? options : Value;
    }
}

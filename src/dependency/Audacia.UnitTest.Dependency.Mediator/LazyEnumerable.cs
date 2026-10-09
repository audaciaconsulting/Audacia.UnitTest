using System.Collections;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// A sequence whose items are only created when it is first enumerated, so a service the test supplies after
/// configuring the mediator is still used by the items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
/// <param name="gate">
/// Held while the items are created. It is taken before anything else, so it can be shared with other code that uses the
/// <see cref="TestTargetBuilder"/> without the two waiting on each other.
/// </param>
/// <param name="create">Creates the items.</param>
internal sealed class LazyEnumerable<T>(object gate, Func<IReadOnlyList<T>> create) : IEnumerable<T>
{
    private IReadOnlyList<T>? _items;

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        lock (gate)
        {
            _items ??= create();

            return _items.GetEnumerator();
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

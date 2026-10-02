using System.Collections;

namespace Audacia.UnitTest.Dependency.Mediator;

/// <summary>
/// A sequence whose items are only created when it is first enumerated, so a service the test supplies after
/// configuring the mediator is still used by the items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
/// <param name="create">Creates the items.</param>
internal sealed class LazyEnumerable<T>(Func<IReadOnlyList<T>> create) : IEnumerable<T>
{
    private readonly Lazy<IReadOnlyList<T>> _items = new(create);

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        return _items.Value.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Represents the chain of types currently being constructed, innermost last.
/// </summary>
internal sealed class DependencyChain
{
    /// <summary>
    /// The minimum number of types in the chain for a parent type to exist.
    /// </summary>
    private const int MinimumCountForParent = 2;

    private Stack<Type> Types { get; } = new();

    /// <summary>
    /// Gets the number of types in the chain.
    /// </summary>
    public int Count => Types.Count;

    /// <summary>
    /// Adds the type to the chain.
    /// </summary>
    /// <param name="type">The type being constructed.</param>
    public void Push(Type type)
    {
        Types.Push(type);
    }

    /// <summary>
    /// Removes the most recently added type from the chain.
    /// </summary>
    /// <returns>The type removed from the chain.</returns>
    public Type Pop()
    {
        return Types.Pop();
    }

    /// <summary>
    /// Gets the parent type from the chain, that is the type one above the current type.
    /// </summary>
    /// <returns>The parent type.</returns>
    /// <exception cref="InvalidOperationException">If the chain has no parent type.</exception>
    public Type GetParentType()
    {
        return Types.Count < MinimumCountForParent
            ? throw new InvalidOperationException("Cannot get parent type when only one type is in the chain.")
            : Types.ElementAt(1);
    }

    /// <summary>
    /// Creates a new chain containing the same types as this one, so sibling
    /// branches of the dependency graph do not share mutations.
    /// </summary>
    /// <returns>A copy of this chain.</returns>
    public DependencyChain Clone()
    {
        var newChain = new DependencyChain();
        foreach (var type in Types.Reverse())
        {
            newChain.Types.Push(type);
        }

        return newChain;
    }

    /// <summary>
    /// Gets a readable representation of the chain, outermost first.
    /// </summary>
    /// <returns>The chain as a string.</returns>
    public string ToChainString()
    {
        return string.Join(" > ", Types.Reverse().Select(type => type.Name));
    }
}

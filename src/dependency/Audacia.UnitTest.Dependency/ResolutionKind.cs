namespace Audacia.UnitTest.Dependency;

/// <summary>
/// How the <see cref="TestTargetBuilder"/> chose to supply a dependency.
/// </summary>
public enum ResolutionKind
{
    /// <summary>
    /// Built by a blueprint that was discovered automatically.
    /// </summary>
    Blueprint,

    /// <summary>
    /// A class constructed by the builder, with each of its constructor parameters resolved in turn.
    /// </summary>
    Class,

    /// <summary>
    /// An <c>IOptions</c> wrapping a default instance.
    /// </summary>
    Options,

    /// <summary>
    /// An <c>ILogger</c> that discards everything it is given.
    /// </summary>
    Logger,

    /// <summary>
    /// An interface, supplied by the implementation the builder found by scanning the assemblies in scope.
    /// </summary>
    Interface
}

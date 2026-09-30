namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Describes one dependency the <see cref="TestTargetBuilder"/> chose automatically, so a test can see what it used.
/// Dependencies supplied explicitly, such as with <see cref="TestTargetBuilder.With{TDependency}"/>, are not described.
/// </summary>
/// <param name="RequestedType">The type that was needed.</param>
/// <param name="Kind">How the builder chose to supply it.</param>
/// <param name="Via">The blueprint or dependency source that supplied it, when there was one.</param>
/// <param name="ImplementationType">The concrete type of the instance that was used.</param>
public sealed record DependencyResolution(
    Type RequestedType,
    ResolutionKind Kind,
    Type? Via,
    Type ImplementationType);

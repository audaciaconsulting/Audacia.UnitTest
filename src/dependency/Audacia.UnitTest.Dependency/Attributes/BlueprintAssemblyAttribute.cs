namespace Audacia.UnitTest.Dependency.Attributes;

/// <summary>
/// Used to tell the seeding logic where to find and load custom <see cref="BlueprintDependency{TCommand}"/>s.
/// </summary>
/// <remarks>
/// <para>Initializes a new instance of the <see cref="BlueprintAssemblyAttribute"/> class.</para>
/// </remarks>
/// <param name="name">The name of the assembly.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class BlueprintAssemblyAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    public string Name { get; } = name;
}
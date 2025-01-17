namespace Audacia.UnitTest.Dependency.Attributes;

/// <summary>
/// Used to tell the seeding logic where to find and load custom <see cref="BlueprintDependency{TCommand}"/>s.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class BlueprintAssemblyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueprintAssemblyAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the assembly.</param>
    public BlueprintAssemblyAttribute(string name)
    {
        Name = name;
    }
}
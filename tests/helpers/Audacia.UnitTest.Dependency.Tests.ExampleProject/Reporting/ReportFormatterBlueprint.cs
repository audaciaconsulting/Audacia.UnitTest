namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Reporting;

/// <summary>
/// A blueprint that implements <see cref="IBlueprintDependency{TDependency}"/> directly rather than
/// deriving from <see cref="BlueprintDependency{TDependency}"/>.
/// </summary>
public sealed class ReportFormatterBlueprint : IBlueprintDependency<IReportFormatter>
{
    /// <inheritdoc />
    public IReportFormatter Build()
    {
        return new ReportFormatter();
    }
}

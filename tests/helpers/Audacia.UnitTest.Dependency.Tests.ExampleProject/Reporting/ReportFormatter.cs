namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Reporting;

/// <summary>
/// The only implementation of <see cref="IReportFormatter"/>. Internal so it is not an exported
/// type, meaning the builder cannot find it by scanning for interface implementations.
/// </summary>
internal sealed class ReportFormatter : IReportFormatter
{
    /// <inheritdoc />
    public string Format(string value)
    {
        return $"blueprint:{value}";
    }
}

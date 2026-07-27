namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Reporting;

/// <summary>
/// Consumes an <see cref="IReportFormatter"/>, which can only be supplied by a blueprint.
/// </summary>
/// <remarks>
/// <para>Initializes a new instance of the <see cref="ReportGenerator"/> class.</para>
/// </remarks>
/// <param name="formatter">The formatter used to produce the report.</param>
public sealed class ReportGenerator(IReportFormatter formatter)
{
    /// <summary>
    /// Generates a report for the given value.
    /// </summary>
    /// <param name="value">The value to report on.</param>
    /// <returns>The generated report.</returns>
    public string Generate(string value)
    {
        return formatter.Format(value);
    }
}

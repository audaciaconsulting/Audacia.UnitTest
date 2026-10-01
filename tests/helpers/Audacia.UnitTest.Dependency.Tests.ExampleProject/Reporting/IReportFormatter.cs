namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Reporting;

/// <summary>
/// Formats a report value. Deliberately has no public implementation, so it can only be
/// resolved from a blueprint.
/// </summary>
public interface IReportFormatter
{
    /// <summary>
    /// Formats the given value.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted value.</returns>
    string Format(string value);
}

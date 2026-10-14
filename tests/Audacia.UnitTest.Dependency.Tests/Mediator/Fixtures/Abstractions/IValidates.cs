namespace Audacia.UnitTest.Dependency.Tests.Mediator.Fixtures.Abstractions;

/// <summary>
/// A validator interface that is not FluentValidation's, to show any validation library can be used.
/// </summary>
public interface IValidates<in T>
{
    IEnumerable<string> Problems(T value);
}

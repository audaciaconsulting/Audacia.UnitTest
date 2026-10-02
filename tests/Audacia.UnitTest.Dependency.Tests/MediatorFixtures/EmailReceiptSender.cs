using Audacia.UnitTest.Dependency.Tests.MediatorFixtures.Abstractions;

namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

/// <summary>
/// Stands for a dependency with a side effect that a test does not want to run.
/// </summary>
public sealed class EmailReceiptSender : IReceiptSender
{
    public string Send(string customer)
    {
        throw new InvalidOperationException("An email would have been sent.");
    }
}

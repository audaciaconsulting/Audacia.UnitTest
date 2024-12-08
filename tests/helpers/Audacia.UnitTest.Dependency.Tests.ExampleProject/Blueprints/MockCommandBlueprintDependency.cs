namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Blueprints;

using Commands.Stud;
using Moq;

/// <summary>
/// Blueprint dependency for <see cref="IMockCommandHandler"/>.
/// </summary>
public class MockCommandBlueprintDependency : BlueprintDependency<IMockCommandHandler>
{
    private readonly Mock<IMockCommandHandler>? _mockCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockCommandBlueprintDependency"/> class.
    /// </summary>
    public MockCommandBlueprintDependency()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockCommandBlueprintDependency"/> class, with a mocked instance
    /// <see cref="IMockCommandHandler"/> to be used over default.
    /// </summary>
    /// <param name="mockCommandHandler">Mocked instance of <see cref="IMockCommandHandler"/>.</param>
    public MockCommandBlueprintDependency(Mock<IMockCommandHandler> mockCommandHandler)
    {
        _mockCommandHandler = mockCommandHandler;
    }

    /// <summary>
    /// Gets the default response message for the mocked command.
    /// </summary>
    public static string DefaultResponseMessage => "I like to cook";

    public override IMockCommandHandler Build()
    {
        if (_mockCommandHandler != null)
        {
            return _mockCommandHandler.Object;
        }

        return base.Build();
    }
}
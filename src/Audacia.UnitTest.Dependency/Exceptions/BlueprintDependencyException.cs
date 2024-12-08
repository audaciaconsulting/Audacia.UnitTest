namespace Audacia.UnitTest.Dependency.Exceptions;

public class BlueprintDependencyException : Exception
{
    private BlueprintDependencyException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueprintDependencyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public BlueprintDependencyException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueprintDependencyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">Exception that was thrown to trigger this exception.</param>
    public BlueprintDependencyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
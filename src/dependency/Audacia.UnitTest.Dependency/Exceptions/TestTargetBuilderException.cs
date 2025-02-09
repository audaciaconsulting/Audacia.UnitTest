namespace Audacia.UnitTest.Dependency.Exceptions;

/// <summary>
/// Thrown when a failure occurs during the construction or configuration of a test target. This may happen due to
/// missing dependencies, invalid configurations, or other issues preventing the test target from being properly instantiated.
/// </summary>
public class TestTargetBuilderException : Exception
{
    /// <summary>
    /// Gets the service that caused the builder to throw an exception.
    /// </summary>
    public string Service { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTargetBuilderException"/> class.
    /// </summary>
    public TestTargetBuilderException()
    {
        Service = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTargetBuilderException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public TestTargetBuilderException(string? message) : base(message)
    {
        Service = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTargetBuilderException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="service">The service that caused the <see cref="TestTargetBuilder"/> to exception.</param>
    public TestTargetBuilderException(
        string message,
        string service) : base(message)
    {
        Service = service;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTargetBuilderException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="service">The service that caused the <see cref="TestTargetBuilder"/> to exception.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public TestTargetBuilderException(
        string message,
        string service,
        Exception innerException) : base(message)
    {
        Service = service;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTargetBuilderException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public TestTargetBuilderException(
        string? message,
        Exception? innerException) : base(message, innerException)
    {
        Service = string.Empty;
    }
}
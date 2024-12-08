namespace Audacia.UnitTest.Dependency.Exceptions;

public class TestTargetBuilderException : Exception
{
    public TestTargetBuilderException()
    {
    }

    public TestTargetBuilderException(string? message) : base(message)
    {
    }

    public TestTargetBuilderException(
        string message,
        string service) : base(message)
    {
    }

    public TestTargetBuilderException(
        string message,
        string service,
        Exception innerException) : base(message)
    {
    }

    public TestTargetBuilderException(
        string? message,
        Exception? innerException) : base(message, innerException)
    {
    }
}
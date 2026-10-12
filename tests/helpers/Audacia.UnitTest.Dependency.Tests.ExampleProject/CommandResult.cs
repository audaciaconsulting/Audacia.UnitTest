using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject;

/// <summary>
/// Represents a basic result of handling an <see cref="IRequest{TResponse}"/>, to avoid adding any dependencies.
/// If the result represents a failure then a collection of errors will contain the details.
/// </summary>
/// <param name="errors">The errors.</param>
public class CommandResult(params IEnumerable<string> errors)
{
    /// <summary>
    /// Gets a collection of errors; this collection should only be populated if the result represents a failure.
    /// </summary>
    public IReadOnlyList<string> Errors { get; } = [.. errors];

    /// <summary>
    /// Gets a value indicating whether the handling of the <see cref="IRequest{TResponse}"/> succeeded (true) or failed (false).
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;

    /// <summary>
    /// Creates a <see cref="CommandResult"/> object representing the successful handling of an <see cref="IRequest{TResponse}"/>.
    /// </summary>
    /// <returns>A <see cref="CommandResult"/> object indicating success.</returns>
    public static CommandResult Success()
    {
        return new();
    }

    /// <summary>
    /// Creates a <see cref="CommandResult"/> object representing the failed handling of an <see cref="IRequest{TResponse}"/>.
    /// </summary>
    /// <param name="errors">The errors.</param>
    /// <returns>A <see cref="CommandResult"/> object containing the given <paramref name="errors"/>.</returns>
    public static CommandResult Failure(params IEnumerable<string> errors)
    {
        return new(errors);
    }

    /// <summary>
    /// Creates a <see cref="CommandResult{T}"/> object representing the failed handling of an <see cref="IRequest{TResponse}"/>.
    /// </summary>
    /// <param name="errors">The errors.</param>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <returns>A <see cref="CommandResult{T}"/> object containing the given <paramref name="errors"/>.</returns>
    public static CommandResult<T> Failure<T>(params IEnumerable<string> errors)
    {
        return new(errors);
    }

    /// <summary>
    /// Creates a <see cref="CommandResult{T}"/> object representing the successful handling of an <see cref="IRequest{TResponse}"/>,
    /// with the output of execution given by the <paramref name="output"/>.
    /// </summary>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="output">The output from which to create the result.</param>
    /// <returns>A <see cref="CommandResult{T}"/> object containing the given <paramref name="output"/>.</returns>
    public static CommandResult<T> WithResult<T>(T output)
    {
        return new(output);
    }
}

/// <summary>
/// Represents the result of handling an <see cref="IRequest{TResponse}"/> where the handling requires an output (of type <typeparamref name="T"/>) to be returned to the client.
/// </summary>
/// <typeparam name="T">The type of the output.</typeparam>
public class CommandResult<T> : CommandResult
{
    /// <summary>
    /// Gets the output.
    /// </summary>
    public T Output { get; } = default!;

    /// <summary>
    /// Initializes a new instance of <see cref="CommandResult{T}"/> representing a failure.
    /// </summary>
    /// <param name="errors">The errors.</param>
    public CommandResult(params IEnumerable<string> errors)
        : base(errors)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CommandResult{T}"/> representing a success.
    /// </summary>
    /// <param name="output">The output.</param>
    public CommandResult(T output)
    {
        Output = output;
    }
}

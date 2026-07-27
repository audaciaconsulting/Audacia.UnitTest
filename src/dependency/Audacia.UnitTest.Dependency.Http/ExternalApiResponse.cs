namespace Audacia.UnitTest.Dependency.Http;

/// <summary>
/// A canned response body returned by a mocked external API.
/// </summary>
public class ExternalApiResponse
{
    /// <summary>
    /// Gets or sets the status code returned by the external API.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the message or reason phrase from the API response.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the API call was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the data returned by the API, if any.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets any error details returned by the API.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the response was received.
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Overrides the ToString() method to provide a summary of the response.
    /// </summary>
    /// <returns>Summary of the response as a string.</returns>
    public override string ToString()
    {
        return $"StatusCode: {StatusCode}, Message: {Message}, IsSuccess: {IsSuccess}, ReceivedAt: {ReceivedAt:O}";
    }
}

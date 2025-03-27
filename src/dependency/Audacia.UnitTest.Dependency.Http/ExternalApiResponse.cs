namespace Audacia.UnitTest.Dependency.Http;

public class ExternalApiResponse
{
    /// <summary>
    /// The status code returned by the external API.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// The message or reason phrase from the API response.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Indicates whether the API call was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// The data returned by the API, if any.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Any error details returned by the API.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// The timestamp when the response was received.
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
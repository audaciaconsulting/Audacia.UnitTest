namespace Audacia.UnitTest.Dependency.Http;

/// <summary>
/// A fake OAuth token returned by a mocked token endpoint.
/// </summary>
public class MockApiToken
{
    /// <summary>
    /// Gets or sets the type of the token, for example 'Bearer'.
    /// </summary>
    public required string TokenType { get; set; }

    /// <summary>
    /// Gets or sets the token used to authorise requests to the mocked API.
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the token used to obtain a new access token.
    /// </summary>
    public required string RefreshToken { get; set; }
}

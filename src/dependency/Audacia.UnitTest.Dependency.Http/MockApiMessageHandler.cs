using System.Text.Json;
using RichardSzalay.MockHttp;

namespace Audacia.UnitTest.Dependency.Http;

/// <summary>
/// A message handler that mocks an external API, including its token endpoint.
/// </summary>
public class MockApiMessageHandler : MockHttpMessageHandler
{
    private readonly Uri _tokenUri;

    /// <summary>
    /// Gets the mocked request that serves tokens from the token endpoint.
    /// </summary>
    public MockedRequest MockedTokenRequest { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockApiMessageHandler"/> class.
    /// </summary>
    /// <param name="tokenUri">The address of the token endpoint to mock.</param>
    public MockApiMessageHandler(Uri tokenUri) : base(BackendDefinitionBehavior.Always)
    {
        _tokenUri = tokenUri;

        MockedTokenRequest = SetupAuthorisation();
    }

    /// <summary>
    /// Sets up an expectation that the token endpoint is called.
    /// </summary>
    /// <returns>The mocked token request.</returns>
    public MockedRequest ExpectTokenRequest()
    {
        var tokenUrl = _tokenUri.ToString();
        return this.Expect(tokenUrl)
            .Respond(
                "application/json",
                JsonSerializer.Serialize(
                    new MockApiToken
                    {
                        TokenType = "Bearer",
                        AccessToken = "fake_access_token",
                        RefreshToken = "fake_refresh_token"
                    }));
    }

    private MockedRequest SetupAuthorisation()
    {
        var tokenUrl = _tokenUri.ToString();
        return this.When(tokenUrl)
            .Respond(
                "application/json",
                JsonSerializer.Serialize(
                    new MockApiToken
                    {
                        TokenType = "Bearer",
                        AccessToken = "fake_access_token",
                        RefreshToken = "fake_refresh_token"
                    }));
    }
}
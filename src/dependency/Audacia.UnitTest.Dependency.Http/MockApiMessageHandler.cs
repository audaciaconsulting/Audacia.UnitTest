using System.Text.Json;
using RichardSzalay.MockHttp;

namespace Audacia.UnitTest.Dependency.Http;

public class MockApiMessageHandler : MockHttpMessageHandler
{
    private readonly Uri _tokenUri;

    public MockedRequest MockedTokenRequest { get; private set; }

    public MockApiMessageHandler(Uri tokenUri) : base(BackendDefinitionBehavior.Always)
    {
        _tokenUri = tokenUri;

        MockedTokenRequest = SetupAuthorisation();
    }

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
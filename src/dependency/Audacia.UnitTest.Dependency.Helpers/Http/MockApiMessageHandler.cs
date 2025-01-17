using System.Text.Json;
using RichardSzalay.MockHttp;

namespace Audacia.UnitTest.Dependency.Helpers.Http;

public class MockApiMessageHandler : MockHttpMessageHandler
{
    private readonly string _tokenUrl;

    public MockedRequest MockedTokenRequest { get; private set; }

    public MockApiMessageHandler(string tokenUrl) : base(BackendDefinitionBehavior.Always)
    {
        _tokenUrl = tokenUrl;

        MockedTokenRequest = SetupAuthorisation();
    }

    public MockedRequest ExpectTokenRequest()
    {
        return this.Expect(_tokenUrl)
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
        return this.When(_tokenUrl)
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
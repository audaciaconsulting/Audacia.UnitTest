using Audacia.UnitTest.Dependency.Http.Builders;

namespace Audacia.UnitTest.Dependency.Http.Blueprints;

/// <summary>
/// A blueprint for creating an injectable Http client.
/// </summary>
public class HttpClientBlueprint : BlueprintDependency<HttpClient>
{
    private readonly MockApiMessageHandler? _mockApiMessageHandler;

    public HttpClientBlueprint()
    {
    }

    public HttpClientBlueprint(MockApiMessageHandler? mockApiMessageHandler)
    {
        _mockApiMessageHandler = mockApiMessageHandler;
    }

    public override HttpClient Build()
    {
        if (_mockApiMessageHandler != null)
        {
            var givenHandler = _mockApiMessageHandler.ToHttpClient();
            givenHandler.BaseAddress = new Uri("https://localhost:11111");

            return givenHandler;
        }

        var mockApiMessageHandler = new MockApiMessageHandlerBuilder().OkResponse();

        var httpClient = mockApiMessageHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:11111");

        return httpClient;
    }
}
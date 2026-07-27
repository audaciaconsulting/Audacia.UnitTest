using Audacia.UnitTest.Dependency.Http.Builders;

namespace Audacia.UnitTest.Dependency.Http.Blueprints;

/// <summary>
/// A blueprint for creating an injectable Http client.
/// </summary>
public class HttpClientBlueprint : BlueprintDependency<HttpClient>
{
    private readonly MockApiMessageHandler? _mockApiMessageHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientBlueprint"/> class,
    /// which responds to any request with a successful response.
    /// </summary>
    public HttpClientBlueprint()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientBlueprint"/> class,
    /// using the given handler to determine the responses.
    /// </summary>
    /// <param name="mockApiMessageHandler">The handler to respond with, or <see langword="null"/> to respond with a successful response.</param>
    public HttpClientBlueprint(MockApiMessageHandler? mockApiMessageHandler)
    {
        _mockApiMessageHandler = mockApiMessageHandler;
    }

    /// <summary>
    /// Builds the fake <see cref="HttpClient"/>.
    /// </summary>
    /// <returns>An <see cref="HttpClient"/> backed by the configured handler.</returns>
    public override HttpClient Build()
    {
        if (_mockApiMessageHandler != null)
        {
            var givenHandler = _mockApiMessageHandler.ToHttpClient();
            givenHandler.BaseAddress = new Uri("https://localhost:11111");

            return givenHandler;
        }

#pragma warning disable IDISP001
        var mockApiMessageHandlerBuilder = new MockApiMessageHandlerBuilder();
#pragma warning restore IDISP001
        var mockApiMessageHandler = mockApiMessageHandlerBuilder.OkResponse();

        var httpClient = mockApiMessageHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:11111");

        return httpClient;
    }
}
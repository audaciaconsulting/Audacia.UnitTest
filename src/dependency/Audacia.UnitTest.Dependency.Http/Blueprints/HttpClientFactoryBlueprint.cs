using NSubstitute;

namespace Audacia.UnitTest.Dependency.Http.Blueprints;

/// <summary>
/// A blueprint for creating an injectable Http client factory.
/// </summary>
public class HttpClientFactoryBlueprint : BlueprintDependency<IHttpClientFactory>
{
    private readonly MockApiMessageHandler? _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientFactoryBlueprint"/> class,
    /// whose clients respond to any request with a successful response.
    /// </summary>
    public HttpClientFactoryBlueprint()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientFactoryBlueprint"/> class,
    /// whose clients use the given handler to determine the responses.
    /// </summary>
    /// <param name="handler">The handler the created clients respond with.</param>
    public HttpClientFactoryBlueprint(MockApiMessageHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Builds the fake <see cref="IHttpClientFactory"/>.
    /// </summary>
    /// <returns>An <see cref="IHttpClientFactory"/> that creates clients backed by the configured handler.</returns>
    public override IHttpClientFactory Build()
    {
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
#pragma warning disable IDISP001
        var httpClientFromBlueprint = new HttpClientBlueprint(_handler).Build();
        var httpClient = httpClientFactory.CreateClient();
#pragma warning restore IDISP001
        httpClient.Returns(httpClientFromBlueprint);

        return httpClientFactory;
    }
}
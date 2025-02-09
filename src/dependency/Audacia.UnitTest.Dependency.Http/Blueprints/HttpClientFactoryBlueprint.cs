using NSubstitute;

namespace Audacia.UnitTest.Dependency.Http.Blueprints;

public class HttpClientFactoryBlueprint : BlueprintDependency<IHttpClientFactory>
{
    private readonly MockApiMessageHandler? _handler;

    public HttpClientFactoryBlueprint()
    {
    }

    public HttpClientFactoryBlueprint(MockApiMessageHandler handler)
    {
        _handler = handler;
    }

    public override IHttpClientFactory Build()
    {
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClientFromBlueprint = new HttpClientBlueprint(_handler).Build();
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Returns(httpClientFromBlueprint);

        return httpClientFactory;
    }
}
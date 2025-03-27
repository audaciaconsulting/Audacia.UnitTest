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
#pragma warning disable IDISP001
        var httpClientFromBlueprint = new HttpClientBlueprint(_handler).Build();
        var httpClient = httpClientFactory.CreateClient();
#pragma warning restore IDISP001
        httpClient.Returns(httpClientFromBlueprint);

        return httpClientFactory;
    }
}
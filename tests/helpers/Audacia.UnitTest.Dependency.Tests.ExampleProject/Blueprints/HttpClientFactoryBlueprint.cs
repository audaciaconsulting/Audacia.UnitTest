using Audacia.UnitTest.Dependency.Helpers.Http;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Blueprints;

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
        using var httpClientFromBlueprint = new HttpClientBlueprint().Build();
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.Returns(httpClientFromBlueprint);

        return httpClientFactory;
    }
}
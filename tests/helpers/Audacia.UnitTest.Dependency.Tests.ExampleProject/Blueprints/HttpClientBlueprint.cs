using System;
using System.Net;
using System.Text.Json;
using Audacia.UnitTest.Dependency.Helpers.Http;
using RichardSzalay.MockHttp;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Blueprints;

/// <summary>
/// A blueprint for creating an injectable Http client.
/// </summary>
public class HttpClientBlueprint : BlueprintDependency<HttpClient, MockApiMessageHandler>
{
    private readonly MockApiMessageHandler _mockApiMessageHandler;

    public override MockApiMessageHandler GetInstanceToCustomise()
    {
        return new MockApiMessageHandler("http://localhost:00000/oauth2/v2.0/token");
    }

    public override HttpClient Build()
    {
        var mockApiMessageHandler = new MockApiMessageHandler("http://localhost:00000/oauth2/v2.0/token");

        var defaultResponse = new ExternalApiResponse()
        {
            StatusCode = 200,
            IsSuccess = true,
            ReceivedAt = DateTime.Now,
            ErrorDetails = null,
            Message = "Success"
        };

        mockApiMessageHandler.When("https://localhost:11111").Respond(
            HttpStatusCode.OK,
            "application/json",
            JsonSerializer.Serialize(defaultResponse));

        var httpClient = mockApiMessageHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:11111");

        return httpClient;
    }
}
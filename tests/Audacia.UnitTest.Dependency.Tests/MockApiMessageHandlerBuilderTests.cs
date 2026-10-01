using System.Net;
using System.Text.Json;
using Audacia.UnitTest.Dependency.Http;
using Audacia.UnitTest.Dependency.Http.Blueprints;
using Audacia.UnitTest.Dependency.Http.Builders;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

/// <summary>
/// Covers the response bodies produced by <see cref="MockApiMessageHandlerBuilder"/>, so the body
/// agrees with the status code it is returned alongside.
/// </summary>
public class MockApiMessageHandlerBuilderTests
{
    [Fact]
    public async Task Should_report_success_in_the_body_of_an_okay_response()
    {
        // Act
        var (statusCode, body) = await CallAsync(builder => builder.OkResponse());

        // Assert
        statusCode.ShouldBe(HttpStatusCode.OK);
        body.StatusCode.ShouldBe((int)HttpStatusCode.OK);
        body.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_report_success_in_the_body_of_a_bad_request_response()
    {
        // Act
        var (statusCode, body) = await CallAsync(builder => builder.BadRequestResponse());

        // Assert
        statusCode.ShouldBe(HttpStatusCode.BadRequest);
        body.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        body.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_report_success_in_the_body_of_an_unauthorised_response()
    {
        // Act
        var (statusCode, body) = await CallAsync(builder => builder.UnauthorisedResponse());

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Unauthorized);
        body.StatusCode.ShouldBe((int)HttpStatusCode.Unauthorized);
        body.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_report_success_in_the_body_of_a_forbidden_response()
    {
        // Act
        var (statusCode, body) = await CallAsync(builder => builder.ForbiddenResponse());

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Forbidden);
        body.StatusCode.ShouldBe((int)HttpStatusCode.Forbidden);
        body.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_include_the_given_message_in_the_body_of_a_bad_request_response()
    {
        // Act
        var (_, body) = await CallAsync(builder => builder.BadRequestResponse(message: "Asset name is required"));

        // Assert
        body.Message.ShouldBe("Asset name is required");
    }

    private static async Task<(HttpStatusCode StatusCode, ExternalApiResponse Body)> CallAsync(
        Func<MockApiMessageHandlerBuilder, MockApiMessageHandler> configure)
    {
        using var handlerBuilder = new MockApiMessageHandlerBuilder();
        var handler = configure(handlerBuilder);

        using var client = new HttpClientBlueprint(handler).Build();
        using var response = await client.GetAsync(new Uri("https://localhost:11111"));

        var json = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<ExternalApiResponse>(json);

        return (response.StatusCode, body!);
    }
}

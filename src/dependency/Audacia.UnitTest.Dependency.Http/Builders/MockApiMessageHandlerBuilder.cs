using System.Net;
using System.Text.Json;
using RichardSzalay.MockHttp;

namespace Audacia.UnitTest.Dependency.Http.Builders;

/// <summary>
/// Builder for <see cref="MockApiMessageHandler"/>.
/// </summary>
public class MockApiMessageHandlerBuilder : IDisposable
{
    private readonly MockApiMessageHandler _mockApiMessageHandler = new("http://localhost:00000/oauth2/v2.0/token");

    private bool _isDisposed;

    public MockApiMessageHandler OkResponse(string when = "https://localhost:11111")
    {
        var apiOkResponse = new ExternalApiResponse()
        {
            StatusCode = 200,
            IsSuccess = true,
            ReceivedAt = DateTime.Now,
            ErrorDetails = null,
            Message = "Success"
        };

        _mockApiMessageHandler.When(when).Respond(
            HttpStatusCode.OK,
            "application/json",
            JsonSerializer.Serialize(apiOkResponse));

        return _mockApiMessageHandler;
    }

    public MockApiMessageHandler BadRequestResponse(
        string when = "https://localhost:11111",
        string message = "Bad Request")
    {
        var apiBadRequestResponse = new ExternalApiResponse()
        {
            StatusCode = 400,
            IsSuccess = true,
            ReceivedAt = DateTime.Now,
            ErrorDetails = null,
            Message = message
        };

        _mockApiMessageHandler.When(when).Respond(
            HttpStatusCode.BadRequest,
            "application/json",
            JsonSerializer.Serialize(apiBadRequestResponse));

        return _mockApiMessageHandler;
    }

    public MockApiMessageHandler UnauthorisedResponse(string when = "https://localhost:11111")
    {
        var apiUnauthorisedResponse = new ExternalApiResponse()
        {
            StatusCode = 401,
            IsSuccess = true,
            ReceivedAt = DateTime.Now,
            ErrorDetails = null,
            Message = "Unauthorised"
        };

        _mockApiMessageHandler.When(when).Respond(
            HttpStatusCode.Unauthorized,
            "application/json",
            JsonSerializer.Serialize(apiUnauthorisedResponse));

        return _mockApiMessageHandler;
    }

    public MockApiMessageHandler ForbiddenResponse(string when = "https://localhost:11111")
    {
        var apiForbiddenResponse = new ExternalApiResponse()
        {
            StatusCode = 403,
            IsSuccess = true,
            ReceivedAt = DateTime.Now,
            ErrorDetails = null,
            Message = "Forbidden"
        };

        _mockApiMessageHandler.When(when).Respond(
            HttpStatusCode.Forbidden,
            "application/json",
            JsonSerializer.Serialize(apiForbiddenResponse));

        return _mockApiMessageHandler;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            // free managed resources
            _mockApiMessageHandler.Dispose();
        }

        _isDisposed = true;
    }
}
using System.Net;
using System.Text.Json;
using RichardSzalay.MockHttp;

namespace Audacia.UnitTest.Dependency.Http.Builders;

/// <summary>
/// Builder for <see cref="MockApiMessageHandler"/>.
/// </summary>
public class MockApiMessageHandlerBuilder : IDisposable
{
    private readonly MockApiMessageHandler _mockApiMessageHandler = new(new Uri("http://localhost:00000/oauth2/v2.0/token"));

    private bool _isDisposed;

    /// <summary>
    /// Configures the handler to return a '200 OK' response.
    /// </summary>
    /// <param name="when">The address the response is returned for.</param>
    /// <returns>The configured handler.</returns>
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

    /// <summary>
    /// Configures the handler to return a '400 Bad Request' response.
    /// </summary>
    /// <param name="when">The address the response is returned for.</param>
    /// <param name="message">The message included in the response body.</param>
    /// <returns>The configured handler.</returns>
    public MockApiMessageHandler BadRequestResponse(
        string when = "https://localhost:11111",
        string message = "Bad Request")
    {
        var apiBadRequestResponse = new ExternalApiResponse()
        {
            StatusCode = 400,
            IsSuccess = false,
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

    /// <summary>
    /// Configures the handler to return a '401 Unauthorized' response.
    /// </summary>
    /// <param name="when">The address the response is returned for.</param>
    /// <returns>The configured handler.</returns>
    public MockApiMessageHandler UnauthorisedResponse(string when = "https://localhost:11111")
    {
        var apiUnauthorisedResponse = new ExternalApiResponse()
        {
            StatusCode = 401,
            IsSuccess = false,
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

    /// <summary>
    /// Configures the handler to return a '403 Forbidden' response.
    /// </summary>
    /// <param name="when">The address the response is returned for.</param>
    /// <returns>The configured handler.</returns>
    public MockApiMessageHandler ForbiddenResponse(string when = "https://localhost:11111")
    {
        var apiForbiddenResponse = new ExternalApiResponse()
        {
            StatusCode = 403,
            IsSuccess = false,
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

    /// <summary>
    /// Disposes the handler created by this builder.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the handler created by this builder.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
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
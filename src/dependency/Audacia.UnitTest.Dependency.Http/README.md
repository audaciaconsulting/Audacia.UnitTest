# Audacia.UnitTest.Dependency.Http

Blueprints that give a test target a fake `HttpClient` or `IHttpClientFactory`, so a class that calls an external
API can be tested without one. Companion package to
[`Audacia.UnitTest.Dependency`](../Audacia.UnitTest.Dependency).

Built on [RichardSzalay.MockHttp](https://github.com/richardszalay/mockhttp).

## Installation

```
dotnet add package Audacia.UnitTest.Dependency.Http
```

## Getting started

`IHttpClientFactory` has no implementation of yours for the builder to find, so supply a blueprint for it. With no
arguments, every request gets a `200 OK`:

```csharp
var target = new TestTargetBuilder()
    .WithBlueprint(new HttpClientFactoryBlueprint())
    .Build<AddAssetCommandHandler>();

var result = await target.HandleAsync(new AddAssetCommand("Computer"));
```

Use `HttpClientBlueprint` in exactly the same way for a target that takes an `HttpClient` directly.

### Discovering the blueprints automatically

Blueprints are only discovered in the assemblies your test project declares. Add this package to that list and the
blueprints above are used without passing them to `WithBlueprint`:

```csharp
[assembly: BlueprintAssembly("Audacia.UnitTest.Dependency.Http")]
```

The attribute can be applied more than once. Note that once you declare any assembly, only the declared assemblies
are searched, so list your own blueprint assembly alongside this one.

## Controlling the response

`MockApiMessageHandlerBuilder` produces a handler with a canned response. Pass that handler to either blueprint:

```csharp
using var handlerBuilder = new MockApiMessageHandlerBuilder();
var handler = handlerBuilder.BadRequestResponse();

var target = new TestTargetBuilder()
    .WithBlueprint(new HttpClientFactoryBlueprint(handler))
    .Build<AddAssetCommandHandler>();
```

The builder owns the handler, so dispose the builder — usually with `using`, as above — rather than the handler.

Four responses are available:

| Method | Status |
| --- | --- |
| `OkResponse()` | 200 OK |
| `BadRequestResponse()` | 400 Bad Request |
| `UnauthorisedResponse()` | 401 Unauthorized |
| `ForbiddenResponse()` | 403 Forbidden |

`BadRequestResponse` also takes a `message`, which is placed in the response body.

Each returns the same underlying handler, so calls can be chained to set up more than one address:

```csharp
using var handlerBuilder = new MockApiMessageHandlerBuilder();
handlerBuilder.OkResponse("https://localhost:11111/assets");
var handler = handlerBuilder.ForbiddenResponse("https://localhost:11111/admin");
```

## Which URL is matched

The clients these blueprints build have a base address of `https://localhost:11111`, and every response method
defaults to matching that exact address.

An exact match means a call to `https://localhost:11111/assets` is **not** matched by the default, and an
unmatched request throws `MockHttpMatchException` rather than returning a status code. If you see that exception,
the address being called and the address set up do not agree.

Pass the address your code actually calls:

```csharp
handlerBuilder.OkResponse("https://localhost:11111/assets");
```

MockHttp supports `*` wildcards in the URL, so `"https://localhost:11111/*"` matches any path under the base
address.

## The response body

Responses are `application/json` containing a serialised `ExternalApiResponse`:

```csharp
public class ExternalApiResponse
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public object? Data { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTime ReceivedAt { get; set; }
}
```

If the code under test expects a different shape, configure the handler directly instead — see
[Going further](#going-further).

## The token endpoint

`MockApiMessageHandler` also mocks an OAuth token endpoint, so a client that authenticates before calling the API
does not need it setting up. Handlers from `MockApiMessageHandlerBuilder` mock
`http://localhost:00000/oauth2/v2.0/token`, returning a `MockApiToken`:

```json
{ "TokenType": "Bearer", "AccessToken": "fake_access_token", "RefreshToken": "fake_refresh_token" }
```

To assert the token endpoint was called, use `ExpectTokenRequest()`, or `MockedTokenRequest` for the request that
was set up automatically. Construct `MockApiMessageHandler` yourself to mock a different token address:

```csharp
using var handler = new MockApiMessageHandler(new Uri("https://identity.example.com/oauth2/token"));
```

## Going further

`MockApiMessageHandler` derives from MockHttp's `MockHttpMessageHandler`, so the whole of that library is available
for anything the builder does not cover — matching on headers or content, returning your own body, sequencing
responses, and verifying expectations:

```csharp
using var handler = new MockApiMessageHandler(new Uri("https://localhost:11111/token"));

handler.When(HttpMethod.Post, "https://localhost:11111/assets")
    .WithPartialContent("Computer")
    .Respond(HttpStatusCode.Created, "application/json", """{"id":1}""");

var target = new TestTargetBuilder()
    .WithBlueprint(new HttpClientFactoryBlueprint(handler))
    .Build<AddAssetCommandHandler>();
```

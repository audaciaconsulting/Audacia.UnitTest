namespace Audacia.UnitTest.Dependency.Http;

public class MockApiToken
{
    public required string TokenType { get; set; }

    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }
}
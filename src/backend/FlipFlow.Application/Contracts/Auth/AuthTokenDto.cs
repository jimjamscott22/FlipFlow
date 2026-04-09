namespace FlipFlow.Application.Contracts.Auth;

public sealed class AuthTokenDto
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }
}

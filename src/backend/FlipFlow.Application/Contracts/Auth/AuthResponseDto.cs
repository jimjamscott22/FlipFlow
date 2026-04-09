namespace FlipFlow.Application.Contracts.Auth;

public sealed class AuthResponseDto
{
    public CurrentUserDto User { get; set; } = new();

    public AuthTokenDto Token { get; set; } = new();
}

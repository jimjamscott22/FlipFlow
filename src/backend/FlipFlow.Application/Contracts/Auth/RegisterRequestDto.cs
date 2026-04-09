namespace FlipFlow.Application.Contracts.Auth;

public sealed class RegisterRequestDto
{
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

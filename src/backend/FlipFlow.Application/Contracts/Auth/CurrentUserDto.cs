namespace FlipFlow.Application.Contracts.Auth;

public sealed class CurrentUserDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

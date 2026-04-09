using FlipFlow.Application.Contracts.Auth;

namespace FlipFlow.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    AuthTokenDto CreateToken(Guid userId, string email, string displayName);
}

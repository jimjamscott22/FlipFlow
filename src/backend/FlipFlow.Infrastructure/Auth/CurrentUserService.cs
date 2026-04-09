using System.Security.Claims;
using FlipFlow.Application.Abstractions.Auth;
using Microsoft.AspNetCore.Http;

namespace FlipFlow.Infrastructure.Auth;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? GetUserId()
    {
        var rawValue = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawValue, out var userId) ? userId : null;
    }

    public Guid GetRequiredUserId()
    {
        return GetUserId() ?? throw new UnauthorizedAccessException("The current request is not authenticated.");
    }
}

namespace FlipFlow.Application.Abstractions.Auth;

public interface ICurrentUserService
{
    Guid? GetUserId();

    Guid GetRequiredUserId();
}

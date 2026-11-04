namespace FlipFlow.Api.Security;

/// <summary>
/// When a passcode is configured, requires the access cookie for the API and for
/// uploaded photos. The frontend files themselves stay public so the passcode screen can load.
/// </summary>
public sealed class AccessPasscodeMiddleware(RequestDelegate next, AccessPasscodeService passcode)
{
    public async Task Invoke(HttpContext context)
    {
        if (!passcode.IsRequired || !IsProtected(context.Request.Path) || passcode.IsAuthorized(context))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message = "A passcode is required.", code = "passcode_required" });
    }

    private static bool IsProtected(PathString path)
    {
        if (path.StartsWithSegments("/api/access") || path.StartsWithSegments("/api/health"))
        {
            return false;
        }

        return path.StartsWithSegments("/api") || path.StartsWithSegments("/storage");
    }
}

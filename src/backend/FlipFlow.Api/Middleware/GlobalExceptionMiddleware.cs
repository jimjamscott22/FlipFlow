using System.Text.Json;
using FluentValidation;

namespace FlipFlow.Api.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);
            await WriteErrorResponseAsync(context, exception);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, payload) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                new
                {
                    message = "Validation failed.",
                    errors = validationException.Errors.Select(x => new
                    {
                        field = x.PropertyName,
                        error = x.ErrorMessage
                    })
                }),
            UnauthorizedAccessException unauthorizedException => (
                StatusCodes.Status401Unauthorized,
                new { message = unauthorizedException.Message }),
            _ => (
                StatusCodes.Status500InternalServerError,
                new { message = "An unexpected server error occurred." })
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

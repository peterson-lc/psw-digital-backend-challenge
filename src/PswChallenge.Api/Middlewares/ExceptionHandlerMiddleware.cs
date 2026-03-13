using Microsoft.AspNetCore.Diagnostics;

namespace PswChallenge.Api.Middlewares;

internal sealed class ExceptionHandlerMiddleware(IExceptionHandler handler, ILogger<ExceptionHandlerMiddleware> logger)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await handler.TryHandleAsync(context, ex, context.RequestAborted);
        }
    }
}
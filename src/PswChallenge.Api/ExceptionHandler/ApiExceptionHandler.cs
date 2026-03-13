using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using PswChallenge.Application.Models.Base;

namespace PswChallenge.Api.ExceptionHandler;

[ExcludeFromCodeCoverage]
internal sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var errors = new List<string> { exception.Message };

        if (exception.InnerException != null) errors.Add(exception.InnerException.Message);

        var response = ApiResponseModel<Exception>.Failure(errors);
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
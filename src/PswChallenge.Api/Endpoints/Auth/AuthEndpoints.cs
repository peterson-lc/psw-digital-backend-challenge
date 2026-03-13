using PswChallenge.Application.Models.Auth;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Services.Interfaces;

namespace PswChallenge.Api.Endpoints.Auth;

internal static class AuthEndpoints
{
    extension(IEndpointRouteBuilder builder)
    {
        internal void MapAuthEndpoints()
        {
            builder.MapPost("api/auth/login", LoginAsync)
                .WithName("Login")
                .WithTags("auth")
                .Produces<ApiResponseModel<LoginResponseModel>>()
                .Produces(StatusCodes.Status400BadRequest)
                .AllowAnonymous();
        }
    }

    private static async Task<IResult> LoginAsync(
        LoginRequestModel request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
            return Results.Ok(ApiResponseModel<LoginResponseModel>.Success(response));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.BadRequest(ApiResponseModel<LoginResponseModel>.Failure([ex.Message]));
        }
    }
}
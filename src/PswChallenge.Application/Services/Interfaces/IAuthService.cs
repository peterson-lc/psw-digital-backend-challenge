using PswChallenge.Application.Models.Auth;

namespace PswChallenge.Application.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseModel> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}
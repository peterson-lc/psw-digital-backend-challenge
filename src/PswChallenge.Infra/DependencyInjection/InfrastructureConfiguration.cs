using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PswChallenge.Application.Configuration;
using PswChallenge.Application.Services.Interfaces;
using PswChallenge.Infra.ExternalServices.BrasilApi;
using Refit;

namespace PswChallenge.Infra.DependencyInjection;

public static class InfrastructureConfiguration
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BrasilApiOptions>(configuration.GetSection(BrasilApiOptions.SectionName));
        services.AddExternalServices(configuration);
    }

    private static void AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        var brasilApiOptions = configuration.GetSection(BrasilApiOptions.SectionName).Get<BrasilApiOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{BrasilApiOptions.SectionName}' is missing or invalid.");

        services.AddRefitClient<IBrasilApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(brasilApiOptions.BaseUrl));

        services.AddScoped<IHolidayExternalService, BrasilApiHolidayService>();
    }
}
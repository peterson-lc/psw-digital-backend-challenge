using Microsoft.Extensions.DependencyInjection;
using PswChallenge.Application.Services.Interfaces;
using PswChallenge.Infra.ExternalServices.BrasilApi;
using Refit;

namespace PswChallenge.Infra.DependencyInjection;

public static class InfrastructureConfiguration
{
    extension(IServiceCollection services)
    {
        public void ConfigureInfrastructure()
        {
            services.AddExternalServices();
        }
        
        private void AddExternalServices()
        {
            services.AddRefitClient<IBrasilApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://brasilapi.com.br"));

            services.AddScoped<IHolidayExternalService, BrasilApiHolidayService>();
        }
    }
}
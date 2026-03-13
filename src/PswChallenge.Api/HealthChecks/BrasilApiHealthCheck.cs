using Microsoft.Extensions.Diagnostics.HealthChecks;
using PswChallenge.Infra.ExternalServices.BrasilApi;

namespace PswChallenge.Api.HealthChecks;

/// <summary>
/// Health check for the Brasil API external service.
/// Verifies that the Brasil API is reachable and responding.
/// </summary>
public class BrasilApiHealthCheck(IBrasilApi brasilApi, ILogger<BrasilApiHealthCheck> logger) : IHealthCheck
{
    private const string TimestampKey = "timestamp";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to fetch holidays for the current year as a health check
            var currentYear = DateTime.UtcNow.Year;
            
            // Use a short timeout for health checks
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var holidays = await brasilApi.GetHoliday(currentYear, cts.Token);

            if (holidays == null)
            {
                return HealthCheckResult.Unhealthy(
                    "Brasil API returned null response",
                    data: new Dictionary<string, object>
                    {
                        { "year", currentYear },
                        { TimestampKey, DateTime.UtcNow }
                    });
            }

            return HealthCheckResult.Healthy(
                "Brasil API is responding normally",
                data: new Dictionary<string, object>
                {
                    { "year", currentYear },
                    { "holidayCount", holidays.Count() },
                    { TimestampKey, DateTime.UtcNow }
                });
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "Brasil API health check timed out");
            return HealthCheckResult.Degraded(
                "Brasil API health check timed out",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "timeout", "5 seconds" },
                    { TimestampKey, DateTime.UtcNow }
                });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Brasil API health check failed with HTTP error");
            return HealthCheckResult.Unhealthy(
                "Brasil API is not reachable",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { TimestampKey, DateTime.UtcNow }
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Brasil API health check failed with unexpected error");
            return HealthCheckResult.Unhealthy(
                "Brasil API health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { TimestampKey, DateTime.UtcNow }
                });
        }
    }
}


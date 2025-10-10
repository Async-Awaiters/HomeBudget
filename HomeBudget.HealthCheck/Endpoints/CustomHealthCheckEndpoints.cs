using HomeBudget.HealthCheck.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace HomeBudget.HealthCheck.Endpoints
{
    public static class CustomHealthCheckEndpoints
    {
        public static void MapCustomHealthCheckEndpoints(this WebApplication app)
        {
            app.MapGet("/health-custom", async (HealthCheckFacade facade) =>
            {
                var result = await facade.CheckAllAsync();

                return Results.Json(new
                {
                    overallStatus = result.Values.Any(v =>
                        v?.ToString()?.Contains("Unhealthy", StringComparison.OrdinalIgnoreCase) == true ||
                        v?.ToString()?.Contains("Unreachable", StringComparison.OrdinalIgnoreCase) == true)
                        ? "Unhealthy"
                        : "Healthy",
                    services = result
                });
            });
        }

    }
}

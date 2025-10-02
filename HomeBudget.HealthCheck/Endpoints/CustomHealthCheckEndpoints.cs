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
                    overallStatus = result.Values.Any(r => r.ToString().Contains("Unhealthy"))
                        ? "Unhealthy" : "Healthy",
                    services = result
                });
            });
        }

    }
}

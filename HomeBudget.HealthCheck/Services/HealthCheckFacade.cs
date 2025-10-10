using HomeBudget.HealthCheck.Helpers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HomeBudget.HealthCheck.Services
{
    public class HealthCheckFacade
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string Name, string Uri)> _externalServices;
        private readonly ILogger<HealthCheckFacade> _logger;

        public HealthCheckFacade(HttpClient httpClient, ILogger<HealthCheckFacade> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _externalServices = HealthEnvHelper.GetHealthEndpoints(logger);
        }

        public async Task<Dictionary<string, object>> CheckAllAsync(CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, object>();

            foreach (var service in _externalServices)
            {
                try
                {
                    _logger.LogInformation("Checking {Service} at {Uri}", service.Name, service.Uri);
                    var response = await _httpClient.GetAsync(service.Uri, cancellationToken);

                    var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

                    var status = json.TryGetProperty("status", out var st) ? st.GetString() : "Unknown";
                    var simplifiedEntries = new List<object>();

                    if (json.TryGetProperty("entries", out var entries))
                    {
                        foreach (var entry in entries.EnumerateObject())
                        {
                            simplifiedEntries.Add(new
                            {
                                name = entry.Name,
                                status = entry.Value.TryGetProperty("status", out var eSt) ? eSt.GetString() : "Unknown",
                                description = entry.Value.TryGetProperty("description", out var desc) ? desc.GetString() : null
                            });
                        }
                    }

                    results[service.Name] = new
                    {
                        status,
                        entries = simplifiedEntries
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Health check for {Service} failed", service.Name);

                    results[service.Name] = new
                    {
                        status = "Unreachable",
                        error = ex.Message,
                        entries = new List<object>()
                    };
                }
            }
            return results;
        }
    }
}


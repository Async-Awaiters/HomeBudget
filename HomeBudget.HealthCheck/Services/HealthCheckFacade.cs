using System.Net.Http.Json;
using System.Text.Json;

namespace HomeBudget.HealthCheck.Services
{
    public class HealthCheckFacade
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string Name, string Uri)> _externalServices;

        public HealthCheckFacade(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _externalServices = configuration
                .GetSection("HealthChecksUI:HealthChecks")
                .Get<List<Dictionary<string, string>>>()?
                .Select(x => (Name: x["Name"], Uri: x["Uri"]))
                .ToList()
                ?? new List<(string, string)>();
        }

        public async Task<Dictionary<string, object>> CheckAllAsync(CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, object>();

            foreach (var service in _externalServices)
            {
                try
                {
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


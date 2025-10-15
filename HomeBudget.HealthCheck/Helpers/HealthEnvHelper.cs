using System.Collections;

namespace HomeBudget.HealthCheck.Helpers
{
    public static class HealthEnvHelper
    {
        public static List<(string Name, string Uri)> GetHealthEndpoints(ILogger? logger = null)
        {
            var result = new List<(string, string)>();

            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                string key = env.Key?.ToString() ?? "";
                string? value = env.Value?.ToString();

                if (key.StartsWith("HEALTHURL_", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value))
                {
                    string name = key.Replace("HEALTHURL_", "").Replace("_", " ");
                    result.Add((name, value));
                    logger?.LogInformation("Discovered health endpoint: {Name} → {Uri}", name, value);
                }
            }

            if (result.Count == 0)
            {
                logger?.LogWarning("No HEALTHURL_* environment variables found.");
            }

            return result;
        }
    }
}

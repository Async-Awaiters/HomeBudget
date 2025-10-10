using HomeBudget.HealthCheck.Endpoints;
using HomeBudget.HealthCheck.Services;
using Microsoft.AspNetCore.Builder;
using System.Collections;

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var logger = LoggerFactory.Create(cfg => cfg.AddConsole()).CreateLogger("HealthCheckInit");

// Подключаем стандартные сервисы
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient<HealthCheckFacade>();

// Подключаем HealthChecks UI, полностью через переменные окружения
builder.Services
    .AddHealthChecksUI(options =>
    {
        // Интервалы
        int evalSeconds = int.TryParse(Environment.GetEnvironmentVariable("HEALTH_EVAL_SECONDS"), out var s) ? s : 10;
        int failNotifySeconds = int.TryParse(Environment.GetEnvironmentVariable("HEALTH_NOTIFY_SECONDS"), out var n) ? n : 20;

        options.SetEvaluationTimeInSeconds(evalSeconds);
        options.SetMinimumSecondsBetweenFailureNotifications(failNotifySeconds);

        int registeredCount = 0;

        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            string key = env.Key.ToString() ?? "";

            // Регистрируем только переменные с HEALTHURL_ как endpoint
            if (key.StartsWith("HEALTHURL_", StringComparison.OrdinalIgnoreCase))
            {
                string name = key.Replace("HEALTHURL_", "").Replace("_", " ");
                string? uri = env.Value?.ToString();

                if (!string.IsNullOrWhiteSpace(uri))
                {
                    options.AddHealthCheckEndpoint(name, uri);
                    registeredCount++;
                    logger.LogInformation("Registered health endpoint: {Name} → {Uri}", name, uri);
                }
            }
        }

        if (registeredCount == 0)
        {
            logger.LogError("No HEALTHURL_* environment variables found — HealthCheck UI will start empty.");
        }
        else
        {
            logger.LogInformation("Registered {Count} service(s) for monitoring.", registeredCount);
        }
    })
    .AddInMemoryStorage();

var app = builder.Build();

app.UseRouting();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-api";
});

app.MapCustomHealthCheckEndpoints();

app.Run();

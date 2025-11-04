using HomeBudget.HealthCheck.Endpoints;
using HomeBudget.HealthCheck.Helpers;
using HomeBudget.HealthCheck.Services;
using Microsoft.AspNetCore.Builder;

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

        var endpoints = HealthEnvHelper.GetHealthEndpoints(logger);

        if (endpoints.Count == 0)
        {
            logger.LogError("No HEALTHURL_* environment variables found — HealthCheck UI will start empty.");
        }
        else
        {
            foreach (var (name, uri) in endpoints)
            {
                options.AddHealthCheckEndpoint(name, uri);
            }

            logger.LogInformation("Registered {Count} endpoint(s) for monitoring.", endpoints.Count);
        }
    })
    .AddInMemoryStorage();

var app = builder.Build();

app.UseRouting();

app.UseStaticFiles();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-api";
});

app.MapCustomHealthCheckEndpoints();

app.Run();

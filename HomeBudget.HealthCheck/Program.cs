using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Подключаем HealthChecks и UI из appsettings.json
builder.Services
    .AddHealthChecksUI(options =>
    {
        options.SetEvaluationTimeInSeconds(
            builder.Configuration.GetValue<int>("HealthChecksUI:EvaluationTimeInSeconds", 10));
        options.SetMinimumSecondsBetweenFailureNotifications(
            builder.Configuration.GetValue<int>("HealthChecksUI:MinimumSecondsBetweenFailureNotifications", 20));
    })
    .AddInMemoryStorage();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseRouting();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui"; // UI доступен по адресу http://localhost:5000/health-ui
    options.ApiPath = "/health-api"; // JSON API для UI
});

app.Run();

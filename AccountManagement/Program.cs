using System.Text;
using AccountManagement.EF;
using AccountManagement.EF.Repositories.Interfaces;
using AccountManagement.Middleware;
using AccountManagement.Services;
using AccountManagement.Services.Interfaces;
using AccountManagement.TransactionProcessing;
using AccountManagement.TransactionProcessing.Strategies;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Extensions.Logging;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var logger = LogManager.Setup().GetCurrentClassLogger();

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
        loggingBuilder.AddNLog(config);
    });

// Настройка Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Разрешает все источники (origins)
              .AllowAnyHeader()   // Разрешает все заголовки
              .AllowAnyMethod()   // Разрешает все HTTP-методы (GET, POST, OPTIONS и т.д.)
              .WithExposedHeaders("Authorization", "X-Custom-Header");
    });
});

builder.Services.AddDbContext<AccountManagementContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgreSQL")));

// Репозитории
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();

// Сервисы
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionsService, TransactionsService>();

builder.Services.AddSingleton<ICurrencyConverter>(converter => new CurrencyConverter(
    builder.Configuration["ExternalServicesURLs:ExchangeRatesApi"] ?? string.Empty,
    builder.Configuration["ExternalServicesURLs:DirrectoriesServices"] ?? string.Empty)
);

// Стратегии
builder.Services.AddScoped<ITransactionCheckStrategy, CashCheckStrategy>();
builder.Services.AddScoped<ITransactionCheckStrategy, DebitCardCheckStrategy>();
builder.Services.AddScoped<ITransactionCheckStrategy, CreditCardCheckStrategy>();

builder.Services.AddScoped<ITransactionStrategyFactory, TransactionCheckStrategyFactory>();
builder.Services.AddScoped<ITransactionProcessor, TransactionProcessor>();

// Аутентификация JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSecret = builder.Configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("JWT Secret is not configured. Please set 'Jwt:Secret' in appsettings.json.");
        }

        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        if (string.IsNullOrEmpty(jwtIssuer))
        {
            throw new InvalidOperationException("JWT Issuer is not configured. Please set 'Jwt:Issuer' in appsettings.json.");
        }

        var jwtAudience = builder.Configuration["Jwt:Audience"];
        if (string.IsNullOrEmpty(jwtAudience))
        {
            throw new InvalidOperationException("JWT Audience is not configured. Please set 'Jwt:Audience' in appsettings.json.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "HomeBudget Accounts API",
            Version = "v1",
            Description = "API для работы со счетами."
        });

        c.MapType<Guid>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "uuid",
            Example = OpenApiAnyFactory.CreateFromJson($"\"{Guid.NewGuid()}\"")
        });
    });
}

builder.Services.AddHealthChecks()
    // Проверка БД
    .AddNpgSql(
        builder.Configuration.GetConnectionString("postgreSQL")!,
        name: "PostgreSQL",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "ready"],
        timeout: TimeSpan.FromSeconds(5) // таймаут на соединение
    )
    // Проверка самого сервиса (жив ли процесс)
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "liveness" });

builder.Services
    .AddControllers(options => { options.SuppressAsyncSuffixInActionNames = false; })
    .AddNewtonsoftJson(
        options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

var app = builder.Build();

app.UseCors("AllowAll");

app.UseExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => { options.RouteTemplate = "/openapi/{documentName}.json"; });
    app.MapScalarApiReference(options =>
    {
        options.Title = "HomeBudget Account Management API";
        options.Theme = ScalarTheme.Laserwave;
    });
}

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("liveness"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

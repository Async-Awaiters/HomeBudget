using HealthChecks.UI.Client;
using HomeBudget.AuthService.EF.Data;
using HomeBudget.AuthService.EF.Repositories;
using HomeBudget.AuthService.EF.Repositories.Interfaces;
using HomeBudget.AuthService.Endpoints;
using HomeBudget.AuthService.Middleware;
using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.Security;
using HomeBudget.AuthService.Services.Implementations;
using HomeBudget.AuthService.Services.Interfaces;
using HomeBudget.AuthService.ValidationHelpers;
using HomeBudget.AuthService.ValidationHelpers.Implementations;
using HomeBudget.AuthService.ValidationHelpers.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Логирование
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
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

// Подключение к БД
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgreSQL")));

builder.Services.AddHealthChecks()
// Проверка БД
    .AddNpgSql(
        builder.Configuration.GetConnectionString("postgreSQL")!,
        name: "PostgreSQL",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "ready" },
        timeout: TimeSpan.FromSeconds(5) // таймаут на соединение
    )
    // Проверка самого сервиса (жив ли процесс)
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "liveness" });

// Репозитории, сервисы и хелперы
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenBuilder, JwtTokenBuilder>();
builder.Services.AddScoped<IRequestValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IRequestValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IUpdateRequestValidator<UpdateRequest>, UpdateRequestValidator>();

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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HomeBudget AuthService API",
        Version = "v1",
        Description = "API для аутентификации и управления пользователями."
    });

    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Example = OpenApiAnyFactory.CreateFromJson("\"2025-06-18\"")
    });

    c.MapType<Guid>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "uuid",
        Example = OpenApiAnyFactory.CreateFromJson("\"00000000-0000-0000-0000-000000000000\"")
    });

    // Добавляем описание авторизации для Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT-токен в формате: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseExceptionMiddleware();

// Настраиваем Swagger и Scalar
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => { options.RouteTemplate = "/openapi/{documentName}.json"; });

    // Настраиваем Scalar
    app.MapScalarApiReference(options =>
    {
        options.Title = "HomeBudget AuthService API";
        options.Theme = ScalarTheme.Laserwave;
    });
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("liveness"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();

app.Run();
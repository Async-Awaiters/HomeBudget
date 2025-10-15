using HealthChecks.UI.Client;
using HomeBudget.Directories;
using HomeBudget.Directories.EF.DAL;
using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.Endpoints;
using HomeBudget.Directories.Middleware;
using HomeBudget.Directories.Models.Categories.Requests;
using HomeBudget.Directories.Services.Implementations;
using HomeBudget.Directories.Services.Interfaces;
using HomeBudget.Directories.ValidationHelpers.Implementations;
using HomeBudget.Directories.ValidationHelpers.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();

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

builder.Services.AddDbContext<DirectoriesContext>(options =>
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

// Регистрация сервисов
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IRequestValidator<CreateCategoryRequest>, CreateRequestValidator>();
builder.Services.AddScoped<IRequestValidator<UpdateCategoryRequest>, UpdateRequestValidator>();

// Регистрация репозиториев
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();

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

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "HomeBudget Directories API",
            Version = "v1",
            Description = "API для работы со справочниками."
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
}

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
        options.Title = "HomeBudget API";
        options.Theme = ScalarTheme.BluePlanet;
    });
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("liveness"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapCurrencyEndpoints();
app.MapCategoryEndpoints();

app.Run();
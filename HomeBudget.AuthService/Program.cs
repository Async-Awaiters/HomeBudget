using HomeBudget.AuthService.EF.Data;
using HomeBudget.AuthService.EF.Repositories;
using HomeBudget.AuthService.EF.Repositories.Interfaces;
using HomeBudget.AuthService.Endpoints;
using HomeBudget.AuthService.Services.Implementations;
using HomeBudget.AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
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

// Подключение к БД
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgreSQL")));

// Репозитории и сервисы
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Аутентификация JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]))
        };

        // Извлекаем токен из куки
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["auth_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
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
        Description = "API для аутентификации и управления пользователями в системе HomeBudget."
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

// Настраиваем Swagger и Scalar
app.UseSwagger(options =>
{
    options.RouteTemplate = "openapi/{documentName}.json";
    options.SerializeAsV2 = false; // Убедимся, что используется OpenAPI 3.0
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "HomeBudget AuthService API v1");
    c.RoutePrefix = "swagger"; // Swagger UI будет доступен по /swagger
});

// Настраиваем Scalar
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("HomeBudget AuthService API")
        .WithTheme(ScalarTheme.Moon)
        .WithSidebar(true)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithOpenApiRoutePattern("/openapi/v1.json"); // Указываем путь к OpenAPI JSON
});

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger(options =>
//    {
//        options.RouteTemplate = "/openapi/{documentName}.json";
//    });
//    app.MapScalarApiReference(options =>
//    {
//        options.Title = "authService API";
//        options.ShowSidebar = true;
//    });
//}

//app.UseHttpsRedirection();

//// Применение миграций при запуске
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    dbContext.Database.Migrate();
//}

//app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();

app.Run();
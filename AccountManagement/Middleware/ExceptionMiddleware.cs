using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using AccountManagement.EF.Exceptions;
using AccountManagement.Exceptions;

namespace AccountManagement.Middleware;

/// <summary>
/// Middleware для обработки исключений в приложении AccountManagement.
/// Логирует непредвиденные ошибки и возвращает клиенту соответствующие HTTP-ответы.
/// </summary>
public class ExceptionMiddleware
{
    /// <summary>
    /// Делегат запроса для передачи управления следующему middleware в pipeline.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// Логгер для записи информации об ошибках.
    /// </summary>
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ExceptionMiddleware"/>.
    /// </summary>
    /// <param name="next">Делегат запроса для передачи управления.</param>
    /// <param name="logger">Логгер для записи информации об ошибках.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Асинхронный метод, вызываемый при обработке HTTP-запроса.
    /// Обрабатывает исключения, возникающие в middleware pipeline.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Обрабатывает исключение, формирует ответ клиенту и записывает лог.
    /// Определяет HTTP-статус и сообщение на основе типа исключения.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="ex">Исключение, которое произошло.</param>
    /// <returns>Задача, представляющая асинхронную операцию записи ответа.</returns>
    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        HttpStatusCode statusCode;
        string message;

        switch (ex)
        {
            case KeyNotFoundException _:
                statusCode = HttpStatusCode.NotFound;
                message = ex.Message;
                break;
            case UnauthorizedAccessException _:
                statusCode = HttpStatusCode.Unauthorized;
                message = ex.Message;
                break;
            case ArgumentException _:
                statusCode = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;
            case AccessDeniedException _:
                statusCode = HttpStatusCode.Forbidden;
                message = ex.Message;
                break;
            case JsonException jsonEx:
                statusCode = HttpStatusCode.BadRequest;
                message = $"Invalid JSON data: {jsonEx.Message}";
                break;
            case BadHttpRequestException badHttpEx when badHttpEx.InnerException is JsonException jsonEx:
                statusCode = HttpStatusCode.BadRequest;
                message = $"Invalid JSON data: {jsonEx.Message}";
                break;
            case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                message = valEx.Message;
                break;
            case EntityNotFoundException _:
                statusCode = HttpStatusCode.NotFound;
                message = ex.Message;
                break;
            case EntityAlreadyExistsException _:
                statusCode = HttpStatusCode.Conflict;
                message = ex.Message;
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred. Please try again later.";
                break;
        }

        var response = new
        {
            Status = (int)statusCode,
            Error = message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

/// <summary>
/// Extension method для добавления middleware в pipeline/
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}

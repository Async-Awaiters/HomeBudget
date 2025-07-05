using HomeBudget.AuthService.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace HomeBudget.AuthService.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

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

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode;
            string message;

            switch (ex)
            {
                case DuplicateUserException _:
                    statusCode = HttpStatusCode.Conflict;
                    message = ex.Message;
                    break;
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
                case JsonException jsonEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = $"Invalid JSON data: {jsonEx.Message}";
                    break;
                case BadHttpRequestException badHttpEx when badHttpEx.InnerException is JsonException jsonEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = $"Invalid JSON data: {jsonEx.Message}";
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

    // Extension method для добавления middleware в pipeline
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
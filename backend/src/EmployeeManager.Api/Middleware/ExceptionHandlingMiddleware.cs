using System.Net;
using System.Text.Json;

namespace EmployeeManager.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception");

            var (statusCode, message) = ex switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Unexpected error.")
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var payload = JsonSerializer.Serialize(new { message });
            await context.Response.WriteAsync(payload);
        }
    }
}

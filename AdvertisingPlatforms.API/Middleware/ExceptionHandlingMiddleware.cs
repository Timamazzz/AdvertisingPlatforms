using System.Text.Json;

namespace AdvertisingPlatforms.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            InvalidOperationException => StatusCodes.Status503ServiceUnavailable,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidDataException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var response = new
        {
            error = exception.Message,
            status = statusCode
        };

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
using System.Net;
using System.Text.Json;

namespace AdvertisingPlatforms.API.Middleware;

/// <summary>
/// Middleware для глобальной обработки исключений и логирования ошибок.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка обработки запроса: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        response.StatusCode = exception switch
        {
            InvalidDataException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.ServiceUnavailable,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var errorResponse = new { error = exception.Message };
        var json = JsonSerializer.Serialize(errorResponse);

        await response.WriteAsync(json);
    }
}
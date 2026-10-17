namespace UI.Middleware;

/// <summary>
/// Logs all requests whose response has a status code other than 2XX
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
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
        catch
        {
            // An unhandled exception results in a 500 status code response
            LogRequest(context, StatusCodes.Status500InternalServerError);
            throw;
        }

        LogRequest(context, context.Response.StatusCode);
    }

    private void LogRequest(HttpContext context, int statusCode)
    {
        if (statusCode is < StatusCodes.Status200OK or >= StatusCodes.Status300MultipleChoices)
        {
            _logger.LogInformation("request {Url} returns {StatusCode}",
                context.Request.Path, statusCode);
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}

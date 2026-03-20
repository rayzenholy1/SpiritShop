using System.Diagnostics;

namespace SpiritShop.API.Middleware;

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
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        _logger.LogInformation(
            "➡ {Method} {Path}{QueryString} started",
            request.Method,
            request.Path,
            request.QueryString);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var level = statusCode >= 500 ? LogLevel.Error
                      : statusCode >= 400 ? LogLevel.Warning
                      : LogLevel.Information;

            _logger.Log(level,
                "⬅ {Method} {Path} → {StatusCode} in {Elapsed}ms",
                request.Method,
                request.Path,
                statusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}

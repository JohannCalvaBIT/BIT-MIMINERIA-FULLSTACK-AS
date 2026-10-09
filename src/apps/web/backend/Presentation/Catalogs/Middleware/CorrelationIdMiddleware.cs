using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;

namespace Presentation.Catalogs.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out StringValues correlationId) ||
            StringValues.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Response.Headers[HeaderName] = correlationId.ToString();
        using var scope = _logger.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = correlationId.ToString() });
        await _next(context);
    }
}

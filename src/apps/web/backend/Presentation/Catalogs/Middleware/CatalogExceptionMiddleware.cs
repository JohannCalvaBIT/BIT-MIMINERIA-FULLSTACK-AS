using System.Net;
using System.Text.Json;
using Domain.Catalogs.Exceptions;

namespace Presentation.Catalogs.Middleware;

public sealed class CatalogExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CatalogExceptionMiddleware> _logger;

    public CatalogExceptionMiddleware(RequestDelegate next, ILogger<CatalogExceptionMiddleware> logger)
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
        catch (DuplicateCodeException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.Conflict, "DUPLICATE_CODE", ex.Message);
        }
        catch (DuplicateDescriptionException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.Conflict, "DUPLICATE_DESCRIPTION", ex.Message);
        }
        catch (CatalogInUseException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.Conflict, "CATALOG_IN_USE", ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "VALIDATION_ERROR", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, "NOT_FOUND", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string code, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var payload = new { success = false, error = new { code, message } };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

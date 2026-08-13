using System.Text.Json;
using AssignmentManagement.Domain.Exceptions;
using FluentValidation;

namespace AssignmentManagement.API.Middleware;

/// <summary>
/// Global exception handler. Translates domain and validation exceptions into
/// consistent JSON error responses with the correct HTTP status codes.
/// </summary>
public class ExceptionHandlingMiddleware
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, errors) = ex switch
        {
            ValidationException v => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                v.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            NotFoundException => (StatusCodes.Status404NotFound, ex.Message, null),
            ForbiddenException => (StatusCodes.Status403Forbidden, ex.Message, null),
            ConflictException => (StatusCodes.Status409Conflict, ex.Message, null),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, ex.Message, null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);
        else
            _logger.LogWarning("Request failed ({Status}) on {Path}: {Message}", status, context.Request.Path, ex.Message);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status,
            title,
            traceId = context.TraceIdentifier,
            errors = errors as object
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(json);
    }
}

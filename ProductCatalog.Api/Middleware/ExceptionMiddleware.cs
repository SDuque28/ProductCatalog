using System.Text.Json;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Exceptions;

namespace ProductCatalog.Api.Middleware;

/// <summary>
/// Handles unhandled exceptions and returns standardized JSON error responses.
/// </summary>
public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger used to record unhandled exceptions.</param>
    /// <param name="environment">The host environment used to control error details exposure.</param>
    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Executes the middleware for the current HTTP request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing the request.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("The response has already started, so the exception middleware will not modify the response.");
            throw exception;
        }

        var statusCode = GetStatusCode(exception);
        var response = CreateErrorResponse(statusCode, exception);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await JsonSerializer.SerializeAsync(context.Response.Body, response, SerializerOptions);
    }

    private ErrorResponseDto CreateErrorResponse(int statusCode, Exception exception)
    {
        return new ErrorResponseDto
        {
            StatusCode = statusCode,
            Message = GetMessage(statusCode, exception),
            Details = GetDetails(exception),
            Timestamp = DateTime.UtcNow
        };
    }

    private string GetMessage(int statusCode, Exception exception)
    {
        return statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception.Message;
    }

    private string? GetDetails(Exception exception)
    {
        if (!_environment.IsDevelopment())
        {
            return null;
        }

        return exception.InnerException?.Message;
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

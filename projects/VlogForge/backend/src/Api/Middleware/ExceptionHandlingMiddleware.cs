using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using VlogForge.Application.Common.Exceptions;

namespace VlogForge.Api.Middleware;

/// <summary>
/// Middleware for handling exceptions globally and returning appropriate HTTP responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly Action<ILogger, string, Exception?> LogUnhandledException =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2000, "UnhandledException"),
            "Unhandled exception occurred: {Message}");

    private static readonly Action<ILogger, string, Exception?> LogHandledException =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2001, "HandledException"),
            "Handled exception occurred: {Message}");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Type = "ValidationError",
                    Title = "Validation Failed",
                    Status = (int)HttpStatusCode.BadRequest,
                    Errors = validationException.Errors.SelectMany(e => 
                        e.Value.Select(v => new ErrorDetail { Field = e.Key, Message = v })).ToList()
                }),

            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Type = "NotFound",
                    Title = "Resource Not Found",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = notFoundException.Message
                }),

            ForbiddenAccessException => (
                HttpStatusCode.Forbidden,
                new ErrorResponse
                {
                    Type = "Forbidden",
                    Title = "Access Denied",
                    Status = (int)HttpStatusCode.Forbidden,
                    Detail = "You do not have permission to access this resource."
                }),

            Application.Common.Exceptions.UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse
                {
                    Type = "Unauthorized",
                    Title = "Authentication Required",
                    Status = (int)HttpStatusCode.Unauthorized,
                    Detail = "Authentication is required to access this resource."
                }),

            UnauthorizedException unauthorizedException => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse
                {
                    Type = "Unauthorized",
                    Title = "Authentication Failed",
                    Status = (int)HttpStatusCode.Unauthorized,
                    Detail = unauthorizedException.Message
                }),

            ConflictException conflictException => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    Type = "Conflict",
                    Title = "Resource Conflict",
                    Status = (int)HttpStatusCode.Conflict,
                    Detail = conflictException.Message
                }),

            InvalidOperationException invalidOpException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Type = "InvalidOperation",
                    Title = "Invalid Operation",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = invalidOpException.Message
                }),

            ArgumentException argumentException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Type = "BadRequest",
                    Title = "Invalid Argument",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = argumentException.Message
                }),

            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    Type = "InternalServerError",
                    Title = "Internal Server Error",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = _environment.IsDevelopment() || _environment.EnvironmentName == "Testing"
                        ? $"{exception.GetType().Name}: {exception.Message}"
                        : "An unexpected error occurred. Please try again later."
                })
        };

        // Log the exception
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            LogUnhandledException(_logger, exception.Message, exception);
        }
        else
        {
            LogHandledException(_logger, exception.Message, exception);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(response, JsonOptions);

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// RFC 7807 Problem Details response.
/// </summary>
public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public List<ErrorDetail> Errors { get; set; } = new();
}

/// <summary>
/// Individual error detail.
/// </summary>
public class ErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

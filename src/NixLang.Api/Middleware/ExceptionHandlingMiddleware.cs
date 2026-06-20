using System.Text.Json;
using FluentValidation;
using NixLang.Application.Common.Exceptions;

namespace NixLang.Api.Middleware;

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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            ValidationException valEx => (
                StatusCodes.Status400BadRequest,
                CreateValidationFailureResponse(valEx)),
            
            EmailAlreadyExistsException emailEx => (
                StatusCodes.Status409Conflict,
                CreateErrorResponse("Conflict", emailEx.Message)),

            InvalidCredentialsException credEx => (
                StatusCodes.Status401Unauthorized,
                CreateErrorResponse("Unauthorized", credEx.Message)),

            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                CreateErrorResponse("Bad Request", argEx.Message)),

            _ => (
                StatusCodes.Status500InternalServerError,
                CreateErrorResponse("Internal Server Error", "An unexpected error occurred on the server."))
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static object CreateValidationFailureResponse(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new
        {
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Errors = errors
        };
    }

    private static object CreateErrorResponse(string title, string detail)
    {
        return new
        {
            Title = title,
            Status = contextStatusCode(title),
            Detail = detail
        };
    }

    private static int contextStatusCode(string title)
    {
        return title switch
        {
            "Conflict" => StatusCodes.Status409Conflict,
            "Bad Request" => StatusCodes.Status400BadRequest,
            "Unauthorized" => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

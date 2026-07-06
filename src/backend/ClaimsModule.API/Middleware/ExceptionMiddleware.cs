using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClaimsModule.Application.Common.Exceptions;

namespace ClaimsModule.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case ValidationException validationException:
                code = HttpStatusCode.UnprocessableEntity; // 422
                result = JsonSerializer.Serialize(new { 
                    message = validationException.Message, 
                    errors = validationException.Errors 
                });
                _logger.LogWarning("Validation failures: {Errors}", result);
                break;
            case NotFoundException notFoundException:
                code = HttpStatusCode.NotFound; // 404
                result = JsonSerializer.Serialize(new { 
                    message = notFoundException.Message 
                });
                _logger.LogWarning("Entity not found: {Message}", notFoundException.Message);
                break;
            case DbUpdateConcurrencyException dbEx:
                code = HttpStatusCode.Conflict; // 409
                result = JsonSerializer.Serialize(new { 
                    message = "Concurrency conflict detected. The record has been modified by another user. Please refresh and try again." 
                });
                _logger.LogError(dbEx, "Concurrency conflict during DB update.");
                break;
            default:
                result = JsonSerializer.Serialize(new { 
                    message = "An unexpected error occurred. Please contact system support.", 
                    details = exception.Message 
                });
                _logger.LogError(exception, "An unexpected error occurred.");
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        await context.Response.WriteAsync(result);
    }
}

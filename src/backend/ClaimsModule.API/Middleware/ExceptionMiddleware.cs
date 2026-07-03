using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ClaimsModule.Application.Common.Exceptions;

namespace ClaimsModule.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
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
                break;
            case NotFoundException notFoundException:
                code = HttpStatusCode.NotFound; // 404
                result = JsonSerializer.Serialize(new { 
                    message = notFoundException.Message 
                });
                break;
            case DbUpdateConcurrencyException:
                code = HttpStatusCode.Conflict; // 409
                result = JsonSerializer.Serialize(new { 
                    message = "Concurrency conflict detected. The record has been modified by another user. Please refresh and try again." 
                });
                break;
            default:
                result = JsonSerializer.Serialize(new { 
                    message = "An unexpected error occurred. Please contact system support.", 
                    details = exception.Message 
                });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}

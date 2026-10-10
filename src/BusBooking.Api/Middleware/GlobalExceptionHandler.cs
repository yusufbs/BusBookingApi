using System.Diagnostics;
using BusBooking.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, title, detail, type) = exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "One or more validation errors occurred.",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            ),
            SeatAlreadyBookedException => (
                StatusCodes.Status409Conflict,
                "Seat Conflict",
                exception.Message,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8"
            ),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                exception.Message,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred while processing your request.",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
            )
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning("Handled business exception: {Message}. TraceId: {TraceId}", exception.Message, traceId);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        if (exception is ValidationException validationEx)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors;
        }
        else if (exception is SeatAlreadyBookedException seatEx)
        {
            problemDetails.Extensions["conflictingSeats"] = seatEx.ConflictingSeats;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}

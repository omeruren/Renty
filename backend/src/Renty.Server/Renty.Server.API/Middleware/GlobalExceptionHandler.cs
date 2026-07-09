using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Renty.Server.Application.Common.Exceptions;

namespace Renty.Server.API.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            BusinessRuleException => (StatusCodes.Status409Conflict, "Conflict"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception occurred");
        else
            logger.LogWarning(exception, "Handled exception: {ExceptionType}", exception.GetType().Name);

        ProblemDetails problemDetails;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            problemDetails = new ValidationProblemDetails(errors)
            {
                Title = title,
                Status = statusCode,
                Detail = "One or more validation errors occurred."
            };
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred. Please try again later."
                    : exception.Message
            };
        }

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

        return true;
    }
}

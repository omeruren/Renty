using Serilog.Context;

namespace Renty.Server.API.Middleware;

/// <summary>
/// Pushes the request's TraceIdentifier into the Serilog log context so every log line written
/// while handling this request — including GlobalExceptionHandler's — carries the same TraceId
/// that's returned to the caller in ProblemDetails, letting support correlate a client-reported
/// error to the exact log lines for that request.
/// </summary>
public sealed class TraceIdLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        context.Response.Headers["X-Trace-Id"] = traceId;

        using (LogContext.PushProperty("TraceId", traceId))
        {
            await next(context);
        }
    }
}

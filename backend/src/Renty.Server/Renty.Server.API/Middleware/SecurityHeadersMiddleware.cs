namespace Renty.Server.API.Middleware;

/// <summary>
/// Adds the response headers required by docs/05-Security-Architecture.md §8. Strict-Transport-Security
/// is handled separately by UseHsts() rather than here, since ASP.NET Core's built-in HSTS middleware
/// already computes it correctly (includeSubDomains, max-age, preload eligibility).
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "0");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), camera=(), microphone=()");

        await next(context);
    }
}

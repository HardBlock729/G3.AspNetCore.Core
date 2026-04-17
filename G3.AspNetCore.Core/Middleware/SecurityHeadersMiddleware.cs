using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace G3.AspNetCore.Core.Middleware;

/// <summary>
/// Middleware that adds standard security headers to every response.
/// CSP policy is more permissive in development to allow Swagger UI and local tooling.
/// </summary>
public sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "no-referrer");

        var csp = environment.IsDevelopment()
            ? "default-src 'self'; " +
              "img-src 'self' data: https:; " +
              "script-src 'self' 'unsafe-inline'; " +
              "style-src 'self' 'unsafe-inline'; " +
              "font-src 'self'; " +
              "connect-src 'self' https://*.amazoncognito.com; " +
              "frame-ancestors 'none'"
            : "default-src 'self'; " +
              "img-src 'self' data: https:; " +
              "script-src 'self'; " +
              "style-src 'self' 'unsafe-inline'; " +
              "font-src 'self'; " +
              "connect-src 'self'; " +
              "frame-ancestors 'none'";

        context.Response.Headers.Append("Content-Security-Policy", csp);
        await next(context);
    }
}

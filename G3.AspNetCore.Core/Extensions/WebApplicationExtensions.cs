using G3.AspNetCore.Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for configuring the core middleware pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Applies the standard middleware pipeline in the correct order:
    /// exception handling → compression → output cache → forwarded headers (prod) →
    /// CORS → security headers → rate limiting → bot protection → correlation ID →
    /// authentication → user context → authorization.
    /// Call MapControllers() and MapG3HealthChecks() separately after this.
    /// </summary>
    public static WebApplication UseG3CoreMiddleware(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseResponseCompression();
        app.UseOutputCache();

        if (!app.Environment.IsDevelopment())
        {
            var forwardedOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedOptions.KnownIPNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedOptions);
        }

        app.UseG3Cors();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseRateLimiter();
        app.UseMiddleware<BotProtectionMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseAuthentication();
        app.UseMiddleware<UserContextMiddleware>();
        app.UseAuthorization();

        return app;
    }
}

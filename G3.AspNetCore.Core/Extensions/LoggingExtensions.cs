using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for configuring cost-optimized logging.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures structured JSON console logging and suppresses high-volume framework log
    /// categories. Outputs one JSON object per log line so CloudWatch Logs Insights can
    /// correlate requests by RequestId, TenantId, and other scope properties.
    /// </summary>
    public static WebApplicationBuilder AddG3Logging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.JsonWriterOptions = new JsonWriterOptions { Indented = false };
        });

        builder.Logging.SetMinimumLevel(
            builder.Environment.IsProduction()
                ? LogLevel.Warning
                : LogLevel.Information);

        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware", LogLevel.Warning);

        return builder;
    }
}

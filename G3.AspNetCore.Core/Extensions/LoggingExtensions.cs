using G3.AspNetCore.Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Text.Json;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for configuring cost-optimized logging.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures logging with a rate-limited JSON console provider and suppresses high-volume
    /// framework log categories. Outputs structured JSON per log line so CloudWatch Logs Insights
    /// can correlate requests by RequestId, TenantId, and other scope properties.
    /// </summary>
    public static WebApplicationBuilder AddG3Logging(this WebApplicationBuilder builder)
    {
        // Register JSON formatter and configure options (AddJsonConsole also adds a provider,
        // which we remove below so we can wrap it in the rate limiter).
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.JsonWriterOptions = new JsonWriterOptions { Indented = false };
        });

        // Remove the provider AddJsonConsole registered; keep the formatter + options.
        builder.Logging.ClearProviders();

        // Re-add as a rate-limited wrapper around ConsoleLoggerProvider.
        builder.Logging.Services.AddSingleton<ILoggerProvider>(sp =>
        {
            var consoleProvider = new ConsoleLoggerProvider(
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<ConsoleLoggerOptions>>(),
                sp.GetServices<ConsoleFormatter>());
            return new RateLimitedLoggerProvider(consoleProvider);
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

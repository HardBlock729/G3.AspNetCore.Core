using G3.AspNetCore.Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for configuring cost-optimized logging.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures logging with a rate-limited console provider and suppresses high-volume
    /// framework log categories. Minimizes log volume in production to reduce costs on
    /// paid logging services (e.g. CloudWatch).
    /// </summary>
    public static WebApplicationBuilder AddG3Logging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Logging.Services.AddSingleton<ILoggerProvider>(sp =>
        {
            var consoleProvider = new ConsoleLoggerProvider(
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<ConsoleLoggerOptions>>());
            return new RateLimitedLoggerProvider(consoleProvider);
        });

        builder.Services.Configure<SimpleConsoleFormatterOptions>(options =>
            options.IncludeScopes = true);

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

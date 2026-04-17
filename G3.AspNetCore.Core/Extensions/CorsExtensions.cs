using System;
using G3.AspNetCore.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for configuring CORS.
/// Registers three policies: Development (permissive), Production (strict whitelist), MobileApp (custom scheme).
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds CORS policies bound from the "Cors" configuration section.
    /// Call <see cref="UseG3Cors"/> on the <see cref="WebApplication"/> to activate the correct policy.
    /// </summary>
    public static WebApplicationBuilder AddG3Cors(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<CorsConfiguration>()
            .Bind(builder.Configuration.GetSection(CorsConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var corsConfig = builder.Configuration
            .GetSection(CorsConfiguration.SectionName)
            .Get<CorsConfiguration>() ?? new CorsConfiguration();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Development", policy =>
            {
                var origins = corsConfig.AllowedOrigins.Length > 0
                    ? corsConfig.AllowedOrigins
                    : ["http://localhost:5000", "http://localhost:3000", "http://localhost:5173"];

                policy.WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders(corsConfig.ExposedHeaders)
                    .SetPreflightMaxAge(TimeSpan.FromHours(corsConfig.PreflightMaxAgeHours));
            });

            options.AddPolicy("Production", policy =>
            {
                if (corsConfig.AllowedOrigins.Length == 0)
                {
                    throw new InvalidOperationException(
                        "CORS AllowedOrigins must be configured in appsettings.json for production.");
                }

                policy.WithOrigins(corsConfig.AllowedOrigins)
                    .WithMethods(corsConfig.AllowedMethods)
                    .WithHeaders(corsConfig.AllowedHeaders)
                    .WithExposedHeaders(corsConfig.ExposedHeaders)
                    .SetPreflightMaxAge(TimeSpan.FromHours(corsConfig.PreflightMaxAgeHours));

                if (corsConfig.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });

            options.AddPolicy("MobileApp", policy =>
                policy.SetIsOriginAllowed(origin =>
                        origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders(corsConfig.ExposedHeaders)
                    .SetPreflightMaxAge(TimeSpan.FromHours(corsConfig.PreflightMaxAgeHours)));
        });

        return builder;
    }

    /// <summary>
    /// Activates the Development or Production CORS policy based on the current environment.
    /// </summary>
    public static WebApplication UseG3Cors(this WebApplication app)
    {
        app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");
        return app;
    }
}

using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for mapping standard health check endpoints.
/// </summary>
public static class HealthCheckEndpointExtensions
{
    /// <summary>
    /// Maps three health check endpoints:
    /// /health — full check (all dependencies),
    /// /health/live — liveness probe (container orchestration),
    /// /health/ready — readiness probe (load balancer).
    /// </summary>
    public static WebApplication MapG3HealthChecks(this WebApplication app)
    {
        static async System.Threading.Tasks.Task WriteResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    exception = e.Value.Exception?.Message,
                    data = e.Value.Data,
                    tags = e.Value.Tags
                }).ToList(),
                totalDuration = report.TotalDuration.TotalMilliseconds
            }, new JsonSerializerOptions { WriteIndented = true });

            await context.Response.WriteAsync(result);
        }

        var statusCodes = new System.Collections.Generic.Dictionary<HealthStatus, int>
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        };

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteResponse,
            ResultStatusCodes = statusCodes
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteResponse,
            ResultStatusCodes = statusCodes
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteResponse,
            ResultStatusCodes = statusCodes
        });

        return app;
    }
}

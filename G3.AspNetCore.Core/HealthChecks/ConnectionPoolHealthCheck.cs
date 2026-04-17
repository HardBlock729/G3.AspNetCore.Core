using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace G3.AspNetCore.Core.HealthChecks;

/// <summary>
/// Health check that validates database connection pool configuration.
/// </summary>
public sealed class ConnectionPoolHealthCheck(IConnectionPoolMonitor poolMonitor) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var stats = await poolMonitor.GetPoolStatsAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "activeConnections", stats.ActiveConnections },
                { "idleConnections", stats.IdleConnections },
                { "totalConnections", stats.TotalConnections },
                { "maxPoolSize", stats.MaxPoolSize },
                { "minPoolSize", stats.MinPoolSize },
                { "utilizationPercent", Math.Round(stats.UtilizationPercent, 2) }
            };

            if (stats.MaxPoolSize <= 0)
            {
                return HealthCheckResult.Unhealthy("Connection pool max size is invalid", data: data);
            }

            if (stats.MinPoolSize < 0 || stats.MinPoolSize > stats.MaxPoolSize)
            {
                return HealthCheckResult.Degraded("Connection pool min size is misconfigured", data: data);
            }

            return HealthCheckResult.Healthy(
                $"Connection pool configured correctly (Min: {stats.MinPoolSize}, Max: {stats.MaxPoolSize})",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check connection pool health", exception: ex);
        }
    }
}

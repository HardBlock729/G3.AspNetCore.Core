using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace G3.AspNetCore.Core.HealthChecks;

/// <summary>
/// Monitors Npgsql connection pool configuration.
/// NOTE: Npgsql does not expose live connection counts via a public API.
/// Use OpenTelemetry metrics (db.client.connection.count) for real-time monitoring.
/// </summary>
public sealed class NpgsqlConnectionPoolMonitor(
    ILogger<NpgsqlConnectionPoolMonitor> logger,
    string connectionString,
    int maxPoolSize,
    int minPoolSize)
    : IConnectionPoolMonitor
{
    public Task<ConnectionPoolStats> GetPoolStatsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var stats = new ConnectionPoolStats
            {
                MaxPoolSize = maxPoolSize,
                MinPoolSize = minPoolSize,
                ActiveConnections = 0,
                IdleConnections = 0,
                TotalConnections = 0
            };

            logger.LogDebug(
                "Connection Pool Configuration — Min: {MinPoolSize}, Max: {MaxPoolSize}",
                stats.MinPoolSize,
                stats.MaxPoolSize);

            return Task.FromResult(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve connection pool statistics");
            return Task.FromResult(new ConnectionPoolStats
            {
                MaxPoolSize = maxPoolSize,
                MinPoolSize = minPoolSize
            });
        }
    }
}

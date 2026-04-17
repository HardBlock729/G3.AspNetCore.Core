using System.Threading;
using System.Threading.Tasks;

namespace G3.AspNetCore.Core.HealthChecks;

/// <summary>
/// Monitors database connection pool statistics.
/// </summary>
public interface IConnectionPoolMonitor
{
    Task<ConnectionPoolStats> GetPoolStatsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Connection pool statistics snapshot.
/// </summary>
public record ConnectionPoolStats
{
    public int ActiveConnections { get; init; }
    public int IdleConnections { get; init; }
    public int TotalConnections { get; init; }
    public int MaxPoolSize { get; init; }
    public int MinPoolSize { get; init; }

    public double UtilizationPercent =>
        MaxPoolSize > 0 ? (double)TotalConnections / MaxPoolSize * 100 : 0;
}

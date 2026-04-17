using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace G3.AspNetCore.Core.Middleware;

/// <summary>
/// Middleware that tracks and blocks IPs that generate excessive 404 errors (bot/scanner detection).
/// This helps reduce costs from automated scanners and protects against reconnaissance attacks.
/// </summary>
public sealed class BotProtectionMiddleware(
    RequestDelegate next,
    ILogger<BotProtectionMiddleware> logger)
{
    private static readonly ConcurrentDictionary<string, NotFoundTracker> NotFoundTrackers = new();

    private static DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(15);

    private const int MaxNotFoundsPerWindow = 10;
    private static readonly TimeSpan TrackingWindow = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15);

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (NotFoundTrackers.TryGetValue(ipAddress, out var tracker) && tracker.IsBlocked())
        {
            logger.LogWarning(
                "Blocked request from suspicious IP: {IpAddress} | Path: {Path} | Blocked until: {BlockedUntil}",
                ipAddress,
                context.Request.Path,
                tracker.BlockedUntil);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }

        await next(context);

        if (context.Response.StatusCode == 404)
        {
            var notFoundTracker = NotFoundTrackers.GetOrAdd(ipAddress, _ => new NotFoundTracker());
            notFoundTracker.RecordNotFound();

            if (notFoundTracker.ShouldBlock())
            {
                notFoundTracker.Block();
                logger.LogWarning(
                    "Blocking IP due to excessive 404s: {IpAddress} | 404 count: {Count} | Blocked for: {Duration} minutes",
                    ipAddress,
                    notFoundTracker.NotFoundCount,
                    BlockDuration.TotalMinutes);
            }
        }

        PerformCleanupIfNeeded();
    }

    private static void PerformCleanupIfNeeded()
    {
        var now = DateTime.UtcNow;
        if (now - _lastCleanup < CleanupInterval)
        {
            return;
        }

        _lastCleanup = now;

        var expiredKeys = NotFoundTrackers
            .Where(kvp => kvp.Value.IsExpired())
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            NotFoundTrackers.TryRemove(key, out _);
        }
    }

    private sealed class NotFoundTracker
    {
        private readonly ConcurrentQueue<DateTime> _notFoundTimestamps = new();
        public DateTime? BlockedUntil { get; private set; }

        public int NotFoundCount => _notFoundTimestamps.Count;

        public void RecordNotFound()
        {
            var now = DateTime.UtcNow;
            _notFoundTimestamps.Enqueue(now);

            while (_notFoundTimestamps.TryPeek(out var oldest) && now - oldest > TrackingWindow)
            {
                _notFoundTimestamps.TryDequeue(out _);
            }
        }

        public bool ShouldBlock() =>
            _notFoundTimestamps.Count >= MaxNotFoundsPerWindow && !IsBlocked();

        public void Block() =>
            BlockedUntil = DateTime.UtcNow.Add(BlockDuration);

        public bool IsBlocked() =>
            BlockedUntil.HasValue && DateTime.UtcNow < BlockedUntil.Value;

        public bool IsExpired()
        {
            if (IsBlocked()) return false;
            if (_notFoundTimestamps.IsEmpty) return true;
            _notFoundTimestamps.TryPeek(out var oldest);
            return DateTime.UtcNow - oldest > TrackingWindow;
        }
    }
}

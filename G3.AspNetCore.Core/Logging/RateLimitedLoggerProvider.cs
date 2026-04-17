using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace G3.AspNetCore.Core.Logging;

/// <summary>
/// Logger provider that rate-limits duplicate log messages to reduce costs on paid logging services (e.g. CloudWatch).
/// Currently rate-limits database authentication errors to once per minute.
/// </summary>
public sealed class RateLimitedLoggerProvider(ILoggerProvider innerProvider) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, RateLimitedLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name =>
            new RateLimitedLogger(innerProvider.CreateLogger(name)));

    public void Dispose()
    {
        _loggers.Clear();
        innerProvider.Dispose();
    }

    public sealed class RateLimitedLogger(ILogger innerLogger) : ILogger
    {
        private readonly TimeSpan _rateLimitWindow = TimeSpan.FromMinutes(1);
        private readonly ConcurrentDictionary<string, DateTime> _lastLogTimes = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            innerLogger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) =>
            innerLogger.IsEnabled(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);

            if (exception != null
                && (exception.Message.Contains("password authentication failed")
                    || exception.Message.Contains("SqlState: 28P01")))
            {
                var key = $"db_auth_error_{exception.GetType().Name}";
                var now = DateTime.UtcNow;
                if (_lastLogTimes.TryGetValue(key, out var lastTime) && now - lastTime < _rateLimitWindow)
                {
                    return;
                }

                _lastLogTimes[key] = now;
                var rateLimitedMessage = $"[Rate Limited - 1/min] {message}";
                innerLogger.Log(logLevel, eventId, rateLimitedMessage, exception, (_, _) => rateLimitedMessage);
                return;
            }

            innerLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}

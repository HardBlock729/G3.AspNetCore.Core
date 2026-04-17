using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace G3.AspNetCore.Core.Middleware;

/// <summary>
/// Middleware that extracts or generates correlation IDs (Request ID and Session ID) and adds logging scopes for request tracking.
/// This middleware runs BEFORE authentication to track all requests, including unauthenticated ones.
/// User context is handled separately by <see cref="UserContextMiddleware"/> which runs AFTER authentication.
/// </summary>
public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    private const string RequestIdHeader = "X-Request-ID";
    private const string SessionIdHeader = "X-Session-ID";

    private static readonly string[] BotPaths =
    [
        "/vendor/phpunit",
        "/wp-admin",
        "/wp-login",
        "/wp-content",
        "/.env",
        "/admin",
        "/phpmyadmin",
        "/.git",
        "/config",
        "/backup",
        "/xmlrpc.php",
        "/wp-includes",
        "/cgi-bin",
        "/shell",
        "/console"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers[RequestIdHeader].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();

        var sessionId = context.Request.Headers[SessionIdHeader].FirstOrDefault();

        context.Items["RequestId"] = requestId;
        context.Items["SessionId"] = sessionId;

        context.Response.Headers.TryAdd(RequestIdHeader, requestId);

        var scopeData = new Dictionary<string, object?>
        {
            ["RequestId"] = requestId,
            ["SessionId"] = sessionId,
            ["Endpoint"] = $"{context.Request.Method} {context.Request.Path}",
            ["HttpMethod"] = context.Request.Method
        };

        using (logger.BeginScope(scopeData))
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Request failed: {Method} {Path} | RequestId: {RequestId} | Error: {ErrorMessage}",
                    context.Request.Method,
                    context.Request.Path,
                    requestId,
                    ex.Message);
                throw;
            }
        }
    }

    private static bool IsBotPath(PathString path)
    {
        var pathValue = path.Value;
        if (string.IsNullOrEmpty(pathValue))
        {
            return false;
        }

        return BotPaths.Any(botPath => pathValue.StartsWith(botPath, StringComparison.OrdinalIgnoreCase));
    }
}

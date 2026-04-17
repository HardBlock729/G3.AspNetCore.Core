using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace G3.AspNetCore.Core.Middleware;

/// <summary>
/// Middleware that extracts the authenticated user's ID from JWT claims and adds it to the request context.
/// This middleware MUST run after UseAuthentication() so that context.User is populated.
/// </summary>
public sealed class UserContextMiddleware(
    RequestDelegate next,
    ILogger<UserContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = "anonymous";
        if (context.User.Identity?.IsAuthenticated == true)
        {
            userId = context.User.FindFirst("sub")?.Value
                     ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? "anonymous";
        }

        context.Items["UserId"] = userId;

        using (logger.BeginScope(new { UserId = userId }))
        {
            await next(context);
        }
    }
}

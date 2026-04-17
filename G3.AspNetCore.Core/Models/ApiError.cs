using System;
using G3.AspNetCore.Core.Exceptions;

namespace G3.AspNetCore.Core.Models;

/// <summary>
/// Strongly-typed error response returned by the API for all non-success responses.
/// Created via the static factory methods; the private constructor prevents ad-hoc construction.
/// </summary>
public sealed class ApiError
{
    /// <summary>Error code identifying the failure type (e.g. "SR_9001").</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>User-facing message suitable for display in the UI.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Technical detail message included only in non-production environments.
    /// Contains the original exception message and any contextual data.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Six-character hex identifier matching the originating exception's EventId.
    /// Included in both the API response and log lines so a specific event can be
    /// located in logs from a user-reported error code.
    /// </summary>
    public string EventId { get; init; } = string.Empty;

    /// <summary>ASP.NET Core trace identifier for the HTTP request.</summary>
    public string? TraceId { get; init; }

    public DateTime Timestamp { get; init; }

    private ApiError() { }

    /// <summary>Creates an <see cref="ApiError"/> from an <see cref="ApiException"/>.</summary>
    public static ApiError FromException(
        ApiException ex,
        string? traceId = null,
        bool includeDetails = false) => new()
    {
        Code = ex.ErrorCode,
        Message = ex.FriendlyMessage,
        Details = includeDetails ? ex.Message : null,
        EventId = ex.EventId,
        TraceId = traceId,
        Timestamp = DateTime.UtcNow
    };

    /// <summary>Creates an <see cref="ApiError"/> for a system (non-custom) exception.</summary>
    public static ApiError FromSystem(
        string code,
        string message,
        string eventId,
        string? details = null,
        string? traceId = null) => new()
    {
        Code = code,
        Message = message,
        Details = details,
        EventId = eventId,
        TraceId = traceId,
        Timestamp = DateTime.UtcNow
    };
}

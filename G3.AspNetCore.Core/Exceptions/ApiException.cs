using System;

namespace G3.AspNetCore.Core.Exceptions;

/// <summary>
/// Base class for all application API exceptions.
/// Every instance carries an <see cref="ErrorCode"/> identifying the failure type and a
/// six-character <see cref="EventId"/> generated at construction time.
/// The EventId appears in both log lines and API responses so a specific event can be
/// located in logs from either a support report or a log search.
/// </summary>
public abstract class ApiException : Exception
{
    /// <summary>Error code identifying the failure type (e.g. "IM_1001").</summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Six-character hex identifier generated per exception instance.
    /// Included in the API response so clients can surface it to users and correlate
    /// with server log entries.
    /// </summary>
    public string EventId { get; }

    /// <summary>User-facing message suitable for display in the UI.</summary>
    public string FriendlyMessage { get; }

    protected ApiException(
        string errorCode,
        string message,
        string friendlyMessage,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        EventId = Guid.NewGuid().ToString("N")[..6].ToUpper();
        FriendlyMessage = friendlyMessage;
    }
}

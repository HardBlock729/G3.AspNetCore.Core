namespace G3.AspNetCore.Core.Exceptions;

/// <summary>
/// System-level error codes used by the global exception handler for standard CLR exceptions.
/// Format: XX_NNNN — two-letter domain prefix, four-digit number.
/// Applications should define their own domain-specific codes in addition to these.
/// </summary>
public static class SystemErrorCodes
{
    public const string Unauthorized = "SR_9001";
    public const string ArgumentNull = "SR_9002";
    public const string InvalidArgument = "SR_9003";
    public const string KeyNotFound = "SR_9004";
    public const string InvalidOperation = "SR_9005";
    public const string Unhandled = "SR_9999";
}

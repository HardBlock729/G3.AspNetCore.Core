using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using G3.AspNetCore.Core.Exceptions;
using G3.AspNetCore.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace G3.AspNetCore.Core.Middleware;

/// <summary>
/// Global exception handler middleware that catches unhandled exceptions and returns
/// consistent <see cref="ApiError"/> responses to clients.
/// Every error is logged with its error code and a unique EventId so it can be located
/// in logs from a user-reported error reference.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            logger.LogWarning("Response has already started, cannot write error response.");
            return;
        }

        context.Response.ContentType = "application/json";

        ApiError error;
        int statusCode;

        if (exception is ApiException apiEx)
        {
            statusCode = StatusCodes.Status500InternalServerError;
            error = ApiError.FromException(
                apiEx,
                traceId: context.TraceIdentifier,
                includeDetails: environment.IsDevelopment());

            logger.LogError(
                apiEx,
                "ERROR [{ErrorCode}] [{EventId}] {Message}",
                apiEx.ErrorCode,
                apiEx.EventId,
                apiEx.Message);
        }
        else
        {
            var eventId = Guid.NewGuid().ToString("N")[..6].ToUpper();

            (statusCode, var code, var friendlyMessage) = exception switch
            {
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    SystemErrorCodes.Unauthorized,
                    "You are not authorized to perform this action."),

                ArgumentNullException => (
                    StatusCodes.Status400BadRequest,
                    SystemErrorCodes.ArgumentNull,
                    "Required information is missing from the request."),

                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    SystemErrorCodes.InvalidArgument,
                    "The request is invalid. Please check your input."),

                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    SystemErrorCodes.KeyNotFound,
                    "The requested resource was not found."),

                InvalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    SystemErrorCodes.InvalidOperation,
                    "The requested operation could not be completed."),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    SystemErrorCodes.Unhandled,
                    "An unexpected error occurred. Please try again.")
            };

            error = ApiError.FromSystem(
                code,
                friendlyMessage,
                eventId,
                details: environment.IsDevelopment() ? exception.Message : null,
                traceId: context.TraceIdentifier);

            logger.LogError(
                exception,
                "ERROR [{ErrorCode}] [{EventId}] {Message}",
                code,
                eventId,
                exception.Message);
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(error);
    }
}

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing and metrics with ASP.NET Core and HTTP client instrumentation.
    /// Exports to console in Development; exports to OTLP when OTEL_EXPORTER_OTLP_ENDPOINT is set.
    /// Health check endpoints are excluded from traces to reduce noise.
    /// </summary>
    public static WebApplicationBuilder AddG3OpenTelemetry(
        this WebApplicationBuilder builder,
        string serviceName,
        string? serviceVersion = null)
    {
        var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                    ["host.name"] = Environment.MachineName
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation(options => options.RecordException = true);

                if (builder.Environment.IsDevelopment())
                    tracing.AddConsoleExporter();

                if (!string.IsNullOrEmpty(otlpEndpoint))
                    tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (builder.Environment.IsDevelopment())
                    metrics.AddConsoleExporter();

                if (!string.IsNullOrEmpty(otlpEndpoint))
                    metrics.AddOtlpExporter();
            });

        return builder;
    }
}

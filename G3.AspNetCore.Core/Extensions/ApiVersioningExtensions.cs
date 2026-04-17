using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for configuring API versioning.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Adds API versioning with URL segment, X-Api-Version header, and api-version query string readers.
    /// Default version is 1.0. Version format is "v1", "v2", etc.
    /// </summary>
    public static WebApplicationBuilder AddG3ApiVersioning(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"),
                    new QueryStringApiVersionReader("api-version"));
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return builder;
    }
}

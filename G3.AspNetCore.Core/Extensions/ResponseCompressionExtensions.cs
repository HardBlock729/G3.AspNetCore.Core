using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for configuring response compression.
/// </summary>
public static class ResponseCompressionExtensions
{
    /// <summary>
    /// Adds Gzip and Brotli response compression at fastest level, including application/json responses.
    /// </summary>
    public static WebApplicationBuilder AddG3ResponseCompression(this WebApplicationBuilder builder)
    {
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
            options.Providers.Add<BrotliCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/json"]);
        });

        builder.Services.Configure<GzipCompressionProviderOptions>(o =>
            o.Level = CompressionLevel.Fastest);

        builder.Services.Configure<BrotliCompressionProviderOptions>(o =>
            o.Level = CompressionLevel.Fastest);

        return builder;
    }
}

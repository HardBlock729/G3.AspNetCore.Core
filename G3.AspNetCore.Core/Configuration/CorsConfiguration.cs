using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace G3.AspNetCore.Core.Configuration;

/// <summary>
/// CORS configuration settings. Bind from the "Cors" section in appsettings.json.
/// </summary>
public sealed class CorsConfiguration : IValidatableObject
{
    public const string SectionName = "Cors";

    [Required(ErrorMessage = "CORS AllowedOrigins must be configured")]
    [MinLength(1, ErrorMessage = "At least one allowed origin must be specified")]
    public string[] AllowedOrigins { get; set; } = [];

    [Required(ErrorMessage = "CORS AllowedMethods must be configured")]
    [MinLength(1, ErrorMessage = "At least one allowed method must be specified")]
    public string[] AllowedMethods { get; set; } = [];

    [Required(ErrorMessage = "CORS AllowedHeaders must be configured")]
    [MinLength(1, ErrorMessage = "At least one allowed header must be specified")]
    public string[] AllowedHeaders { get; set; } = [];

    public string[] ExposedHeaders { get; set; } = [];

    public bool AllowCredentials { get; set; } = true;

    [Range(0, 24, ErrorMessage = "PreflightMaxAgeHours must be between 0 and 24")]
    public int PreflightMaxAgeHours { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AllowCredentials && AllowedOrigins.Any(o => o.Contains('*')))
        {
            yield return new ValidationResult(
                "CORS AllowedOrigins cannot contain wildcards (*) when AllowCredentials is true.",
                [nameof(AllowedOrigins), nameof(AllowCredentials)]);
        }

        foreach (var origin in AllowedOrigins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out _))
            {
                yield return new ValidationResult(
                    $"CORS AllowedOrigin '{origin}' is not a valid URL.",
                    [nameof(AllowedOrigins)]);
            }
        }

        var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS", "HEAD" };
        foreach (var method in AllowedMethods)
        {
            if (!validMethods.Contains(method.ToUpperInvariant()))
            {
                yield return new ValidationResult(
                    $"CORS AllowedMethod '{method}' is not valid. Valid methods: {string.Join(", ", validMethods)}",
                    [nameof(AllowedMethods)]);
            }
        }
    }
}

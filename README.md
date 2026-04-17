# G3.AspNetCore.Core

Reusable middleware, extensions, and utilities for ASP.NET Core Web APIs. Bundles the boilerplate that every production API needs — logging, CORS, rate limiting, compression, versioning, OpenTelemetry, health checks, exception handling, and a full middleware pipeline — into a single NuGet package.

[![Build](https://github.com/HardBlock729/G3.AspNetCore.Core/actions/workflows/ci.yml/badge.svg)](https://github.com/HardBlock729/G3.AspNetCore.Core/actions/workflows/ci.yml)
[![NuGet Package](https://img.shields.io/nuget/v/G3Software.Net.AspNetCore.Core)](https://www.nuget.org/packages/G3Software.Net.AspNetCore.Core)

**Targets:** net8.0 · net9.0 · net10.0

---

## Installation

```
dotnet add package G3Software.Net.AspNetCore.Core
```

---

## Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.AddG3Logging();
builder.AddG3Cors();
builder.AddG3RateLimiting();
builder.AddG3ResponseCompression();
builder.AddG3ApiVersioning();
builder.AddG3OpenTelemetry("MyApi", "1.0.0");

builder.Services.AddHealthChecks();
builder.Services.AddG3FluentValidation(typeof(Program).Assembly);
builder.Services.AddG3CurrentUserModelBinding<AppUser, AppUserResolver>();

var app = builder.Build();

app.UseG3CoreMiddleware();
app.MapControllers();
app.MapG3HealthChecks();

app.Run();
```

---

## Features

### Middleware Pipeline — `UseG3CoreMiddleware()`

Applies middleware in the correct order in a single call:

```
GlobalExceptionHandler → ResponseCompression → OutputCache →
ForwardedHeaders (non-dev) → CORS → SecurityHeaders → RateLimiter →
BotProtection → CorrelationId → Authentication → UserContext → Authorization
```

- **GlobalExceptionHandlerMiddleware** — catches all unhandled exceptions, maps them to HTTP status codes, and returns a consistent `ApiError` JSON response with a unique `eventId` for log correlation.
- **SecurityHeadersMiddleware** — injects `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, and a CSP (relaxed in Development, strict in Production).
- **BotProtectionMiddleware** — blocks IPs that generate 10+ 404s within 5 minutes for 15 minutes.
- **CorrelationIdMiddleware** — propagates or generates `X-Request-ID` and `X-Session-ID`, adds them to the logging scope and response headers.
- **UserContextMiddleware** — extracts the user ID from JWT claims (`sub` / `NameIdentifier`) and stores it in `HttpContext.Items`.

### Exception Handling

Extend `ApiException` to create typed, structured errors:

```csharp
public class RecipeNotFoundException : ApiException
{
    public RecipeNotFoundException(int id)
        : base("SR_1001", $"Recipe {id} was not found.", "Recipe not found.") { }
}
```

Every `ApiException` gets a unique 6-character `eventId` for correlating the response back to a specific log entry.

### Logging — `AddG3Logging()`

- Clears default providers and registers a console logger wrapped in a rate limiter (suppresses repeated DB auth errors to 1/minute).
- Sets minimum level to `Warning` in Production and `Information` in Development.
- Filters out high-volume framework categories (`EFCore.Database.Command`, `AspNetCore.Routing`, etc.) to reduce log costs on paid services like CloudWatch.

### CORS — `AddG3Cors()` / `UseG3Cors()`

Registers three named policies bound from the `Cors` configuration section:

| Policy | Used when | Behavior |
|---|---|---|
| `Development` | `IsDevelopment()` | Allows localhost origins, any method/header |
| `Production` | default | Strict whitelist from config |
| `MobileApp` | opt-in | Allows custom app schemes + localhost |

`UseG3Cors()` automatically selects `Development` or `Production` based on environment.

```json
// appsettings.json
{
  "Cors": {
    "AllowedOrigins": ["https://myapp.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Content-Type", "Authorization"],
    "AllowCredentials": true,
    "ExposedHeaders": ["X-Request-ID"],
    "PreflightMaxAgeHours": 24
  }
}
```

### Rate Limiting — `AddG3RateLimiting()`

- **Global policy:** 100 requests/minute per IP (queue depth 10).
- **`login` policy:** 5 requests/15 minutes per IP — apply with `[EnableRateLimiting("login")]`.
- Returns `429 Too Many Requests` with a `Retry-After` header.

### OpenTelemetry — `AddG3OpenTelemetry(serviceName, serviceVersion?)`

- Traces and metrics with ASP.NET Core + HTTP client instrumentation.
- Excludes `/health` endpoints from traces.
- Console exporter in Development; OTLP exporter when `OTEL_EXPORTER_OTLP_ENDPOINT` is set.

### Health Checks — `MapG3HealthChecks()`

Maps three endpoints with a JSON response writer:

| Endpoint | Tag filter |
|---|---|
| `/health` | all checks |
| `/health/live` | `live` tag |
| `/health/ready` | `ready` tag |

Add your own checks with the standard `IHealthChecksBuilder` API and tag them `live` or `ready` as appropriate.

### API Versioning — `AddG3ApiVersioning()`

Configures `Asp.Versioning.Mvc` with URL segment, query string (`api-version`), and header (`x-api-version`) readers. Default version is `1.0`, format is `'v'VVV`.

### FluentValidation — `AddG3FluentValidation(Assembly)`

Registers FluentValidation with auto-validation for all validators found in the given assembly.

```csharp
builder.Services.AddG3FluentValidation(typeof(Program).Assembly);
```

### Current User Model Binding

Resolve the current authenticated user directly into a controller action parameter.

**1. Implement `ICurrentUserResolver<TUser>`:**

```csharp
public class AppUserResolver : ICurrentUserResolver<AppUser>
{
    public async Task<AppUser?> ResolveAsync(string userId, CancellationToken ct)
        => await _db.Users.FindAsync(userId, ct);
}
```

**2. Register:**

```csharp
builder.Services.AddG3CurrentUserModelBinding<AppUser, AppUserResolver>();
```

**3. Use in controllers:**

```csharp
[HttpGet("me")]
public IActionResult GetProfile([FromCurrentUser] AppUser user) => Ok(user);
```

### Response Compression — `AddG3ResponseCompression()`

Enables Gzip and Brotli compression at `Fastest` level for `application/json` and standard web content types.

---

## Configuration Reference

| Section | Type | Description |
|---|---|---|
| `Cors` | `CorsConfiguration` | Origins, methods, headers, credentials |

---

## Related Packages

- [G3Software.Net.AspNetCore.Aws](https://www.nuget.org/packages/G3Software.Net.AspNetCore.Aws) — AWS Cognito auth, S3/Secrets Manager health checks, Npgsql with RDS secret rotation

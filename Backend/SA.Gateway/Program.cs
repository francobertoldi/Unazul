using System.Threading.RateLimiting;
using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using Shared.Auth;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// YARP Reverse Proxy
// ---------------------------------------------------------------------------
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ---------------------------------------------------------------------------
// Authentication & Authorization (reuses Shared.Auth JWT setup)
// ---------------------------------------------------------------------------
builder.Services.AddUnazulAuth(builder.Configuration);

// ---------------------------------------------------------------------------
// CORS
// ---------------------------------------------------------------------------
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:8080"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---------------------------------------------------------------------------
// Correlation ID
// ---------------------------------------------------------------------------
builder.Services.AddCorrelate(options =>
    options.RequestHeaders = ["X-Correlation-ID"]);

// ---------------------------------------------------------------------------
// Rate Limiting  (fixed window: 100 req / 10 s per client IP)
// ---------------------------------------------------------------------------
var permitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 100);
var windowSeconds = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 10);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ---------------------------------------------------------------------------
// Health Checks  (ping downstream clusters)
// ---------------------------------------------------------------------------
var healthBuilder = builder.Services.AddHealthChecks();

var clusters = builder.Configuration.GetSection("ReverseProxy:Clusters");
foreach (var cluster in clusters.GetChildren())
{
    var address = cluster["Destinations:primary:Address"];
    if (!string.IsNullOrEmpty(address))
    {
        var name = cluster.Key;
        healthBuilder.AddUrlGroup(
            new Uri(new Uri(address), "/health"),
            name: $"downstream-{name}",
            tags: ["downstream"]);
    }
}

// ---------------------------------------------------------------------------
// Build & Middleware
// ---------------------------------------------------------------------------
var app = builder.Build();

app.UseCorrelate();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();

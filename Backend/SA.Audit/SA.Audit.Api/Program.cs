using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using SA.Audit.Api.Endpoints.Extensions;
using SA.Audit.Api.Extensions;
using SA.Audit.Application.DependencyInjection;
using SA.Audit.DataAccess.EntityFramework.DependencyInjection;
using SA.Audit.EventBus;
using SA.Audit.Infrastructure.DependencyInjection;
using Scalar.AspNetCore;
using Shared.Auth;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// OpenAPI / Scalar
// ---------------------------------------------------------------------------
builder.Services.AddOpenApi();

// ---------------------------------------------------------------------------
// Authentication & Authorization (reuses Shared.Auth JWT setup)
// ---------------------------------------------------------------------------
builder.Services.AddUnazulAuth(builder.Configuration);

// ---------------------------------------------------------------------------
// Mediator (source generator)
// ---------------------------------------------------------------------------
builder.Services.AddMediator();

// ---------------------------------------------------------------------------
// Application layer
// ---------------------------------------------------------------------------
builder.Services.AddApplicationServices();

// ---------------------------------------------------------------------------
// DataAccess (EF Core + PostgreSQL)
// ---------------------------------------------------------------------------
builder.Services.AddDataAccessServices(builder.Configuration);

// ---------------------------------------------------------------------------
// Infrastructure
// ---------------------------------------------------------------------------
builder.Services.AddInfrastructureServices();

// ---------------------------------------------------------------------------
// EventBus (MassTransit + RabbitMQ, with NoOp fallback)
// ---------------------------------------------------------------------------
builder.Services.AddEventBusServices(builder.Configuration);

// ---------------------------------------------------------------------------
// Correlation ID
// ---------------------------------------------------------------------------
builder.Services.AddCorrelate(options =>
    options.RequestHeaders = ["X-Correlation-ID"]);

// ---------------------------------------------------------------------------
// OpenTelemetry
// ---------------------------------------------------------------------------
builder.Services.AddAuditOpenTelemetry(builder.Configuration);

// ---------------------------------------------------------------------------
// Health checks
// ---------------------------------------------------------------------------
builder.Services.AddHealthChecks();

// ---------------------------------------------------------------------------
// Build & Middleware
// ---------------------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCorrelate();
app.UseUnazulExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapAuditEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
namespace SA.Audit.Api
{
    public partial class Program;
}

using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using SA.Identity.Api.Endpoints.Extensions;
using SA.Identity.Api.Extensions;
using SA.Identity.Application.DependencyInjection;
using SA.Identity.DataAccess.EntityFramework.DependencyInjection;
using SA.Identity.EventBus;
using SA.Identity.Infrastructure.DependencyInjection;
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
// Infrastructure (JWT, Password)
// ---------------------------------------------------------------------------
builder.Services.AddInfrastructureServices(builder.Configuration);

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
builder.Services.AddIdentityOpenTelemetry(builder.Configuration);

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
app.MapIdentityEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
namespace SA.Identity.Api
{
    public partial class Program;
}

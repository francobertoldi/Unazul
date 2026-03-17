using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using SA.Operations.Api.Endpoints.Extensions;
using SA.Operations.Api.Extensions;
using SA.Operations.Application.DependencyInjection;
using SA.Operations.DataAccess.EntityFramework.DependencyInjection;
using SA.Operations.EventBus;
using SA.Operations.Infrastructure.DependencyInjection;
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
// HttpClient (for inter-service communication)
// ---------------------------------------------------------------------------
builder.Services.AddHttpClient();

// ---------------------------------------------------------------------------
// Infrastructure (file storage, catalog client, config client)
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
builder.Services.AddOperationsOpenTelemetry(builder.Configuration);

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
app.MapOperationsEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
namespace SA.Operations.Api
{
    public partial class Program;
}

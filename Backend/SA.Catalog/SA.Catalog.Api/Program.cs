using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using SA.Catalog.Api.Endpoints.Extensions;
using SA.Catalog.Application.DependencyInjection;
using SA.Catalog.DataAccess.EntityFramework.DependencyInjection;
using SA.Catalog.EventBus;
using SA.Catalog.Infrastructure.DependencyInjection;
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
// Infrastructure (Entity validation stub)
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
app.MapCatalogEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
namespace SA.Catalog.Api
{
    public partial class Program;
}

using Mediator;
using SA.Audit.Api.Mappers.AuditLog;
using SA.Audit.Api.ViewModels.AuditLog;
using SA.Audit.Application.Queries;
using Shared.Contract.Exceptions;

namespace SA.Audit.Api.Endpoints.AuditLog;

public static class AuditLogEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/audit-log")
            .WithTags("AuditLog")
            .WithOpenApi()
            .RequireAuthorization();

        // List audit log entries with filters
        group.MapGet("/", async (
            int? page,
            int? size,
            Guid? user_id,
            string? operation,
            string? module,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? sort,
            string? order,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var tenantId = Guid.Parse(
                httpContext.User.FindFirst("tenant_id")?.Value
                ?? throw new ValidationException("AUD_INVALID_OPERATION", "Tipo de operacion invalido."));

            var result = await mediator.Send(new ListAuditLogQuery(
                tenantId,
                page ?? 1,
                size ?? 20,
                user_id,
                operation,
                module,
                from,
                to,
                sort ?? "occurred_at",
                order ?? "desc"));

            return Results.Ok(AuditMapper.ToAuditLogListResponse(result));
        })
        .Produces<AuditLogListResponse>(200);

        // Export audit log entries
        group.MapGet("/export", async (
            string format,
            Guid? user_id,
            string? operation,
            string? module,
            DateTimeOffset? from,
            DateTimeOffset? to,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var tenantId = Guid.Parse(
                httpContext.User.FindFirst("tenant_id")?.Value
                ?? throw new ValidationException("AUD_INVALID_OPERATION", "Tipo de operacion invalido."));

            var result = await mediator.Send(new ExportAuditLogQuery(
                tenantId,
                format,
                user_id,
                operation,
                module,
                from,
                to));

            return Results.File(result.Data, result.ContentType, result.FileName);
        })
        .Produces<byte[]>(200);
    }
}

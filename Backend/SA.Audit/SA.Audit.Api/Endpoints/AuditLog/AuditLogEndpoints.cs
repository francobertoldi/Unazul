using Mediator;
using SA.Audit.Api.Mappers.AuditLog;
using SA.Audit.Api.ViewModels.AuditLog;
using SA.Audit.Application.Queries;
using Shared.Contract.Models;

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
            try
            {
                var tenantId = Guid.Parse(
                    httpContext.User.FindFirst("tenant_id")?.Value
                    ?? throw new InvalidOperationException("AUD_INVALID_OPERATION"));

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
            }
            catch (InvalidOperationException ex) when (ex.Message == "AUD_INVALID_OPERATION")
            {
                return Results.Json(
                    new ErrorResponse("Operacion invalida.", "AUD_INVALID_OPERATION"),
                    statusCode: 422);
            }
            catch (InvalidOperationException ex) when (ex.Message == "AUD_INVALID_DATE_RANGE")
            {
                return Results.Json(
                    new ErrorResponse("Rango de fechas invalido.", "AUD_INVALID_DATE_RANGE"),
                    statusCode: 422);
            }
        })
        .Produces<AuditLogListResponse>(200)
        .Produces<ErrorResponse>(422);

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
            try
            {
                var tenantId = Guid.Parse(
                    httpContext.User.FindFirst("tenant_id")?.Value
                    ?? throw new InvalidOperationException("AUD_INVALID_OPERATION"));

                var result = await mediator.Send(new ExportAuditLogQuery(
                    tenantId,
                    format,
                    user_id,
                    operation,
                    module,
                    from,
                    to));

                return Results.File(result.Data, result.ContentType, result.FileName);
            }
            catch (InvalidOperationException ex) when (ex.Message == "AUD_INVALID_FORMAT")
            {
                return Results.Json(
                    new ErrorResponse("Formato de exportacion invalido.", "AUD_INVALID_FORMAT"),
                    statusCode: 422);
            }
            catch (InvalidOperationException ex) when (ex.Message == "AUD_INVALID_DATE_RANGE")
            {
                return Results.Json(
                    new ErrorResponse("Rango de fechas invalido.", "AUD_INVALID_DATE_RANGE"),
                    statusCode: 422);
            }
            catch (InvalidOperationException ex) when (ex.Message == "AUD_EXPORT_LIMIT_EXCEEDED")
            {
                return Results.Json(
                    new ErrorResponse("Limite de exportacion excedido.", "AUD_EXPORT_LIMIT_EXCEEDED"),
                    statusCode: 422);
            }
            catch (InvalidOperationException ex) when (ex.Message == "AUD_INVALID_OPERATION")
            {
                return Results.Json(
                    new ErrorResponse("Operacion invalida.", "AUD_INVALID_OPERATION"),
                    statusCode: 422);
            }
        })
        .Produces<byte[]>(200)
        .Produces<ErrorResponse>(422);
    }
}

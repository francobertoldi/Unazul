using Mediator;
using SA.Audit.Application.Dtos;
using SA.Audit.DataAccess.Interface.Repositories;
using SA.Audit.Domain;
using Shared.Contract.Exceptions;
using Shared.Export;

namespace SA.Audit.Application.Queries;

public sealed class ExportAuditLogQueryHandler(
    IAuditLogRepository repository) : IQueryHandler<ExportAuditLogQuery, (byte[] Data, string ContentType, string FileName)>
{
    private static readonly IReadOnlyList<ColumnDefinition<AuditLogDto>> Columns =
    [
        new("Fecha y hora", x => x.OccurredAt),
        new("Usuario", x => x.UserName),
        new("Operacion", x => x.Operation),
        new("Accion", x => x.Action),
        new("Modulo", x => x.Module),
        new("Detalle", x => x.Detail),
        new("IP", x => x.IpAddress),
        new("Entidad", x => x.EntityType),
        new("ID Entidad", x => x.EntityId)
    ];

    public async ValueTask<(byte[] Data, string ContentType, string FileName)> Handle(
        ExportAuditLogQuery query, CancellationToken ct)
    {
        var format = query.Format?.ToLowerInvariant();
        if (format is not "xlsx" and not "csv")
            throw new ValidationException("AUD_INVALID_FORMAT", "Formato de exportacion invalido. Use 'xlsx' o 'csv'.");

        if (query.Operation is not null && !AuditOperationType.IsValid(query.Operation))
            throw new ValidationException("AUD_INVALID_OPERATION", "Tipo de operacion invalido.");

        if (query.From.HasValue && query.To.HasValue && query.From.Value > query.To.Value)
            throw new ValidationException("AUD_INVALID_DATE_RANGE", "Rango de fechas invalido.");

        var count = await repository.CountAsync(
            query.TenantId, query.UserId, query.Operation,
            query.Module, query.From, query.To, ct);

        if (count > 10_000)
            throw new ValidationException("AUD_EXPORT_LIMIT_EXCEEDED", "El limite de exportacion ha sido excedido (maximo 10.000 registros).");

        var items = await repository.ListForExportAsync(
            query.TenantId, query.UserId, query.Operation,
            query.Module, query.From, query.To, ct);

        var dtos = items.Select(a => new AuditLogDto(
            a.Id, a.TenantId, a.UserId, a.UserName,
            a.Operation, a.Module, a.Action, a.Detail,
            a.IpAddress, a.EntityType, a.EntityId,
            a.ChangesJson, a.OccurredAt)).ToList();

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        if (format == "xlsx")
        {
            var bytes = ExportService.ToXlsx(dtos, Columns, "Auditoria");
            return (bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"audit_log_{timestamp}.xlsx");
        }
        else
        {
            var bytes = ExportService.ToCsv(dtos, Columns);
            return (bytes, "text/csv", $"audit_log_{timestamp}.csv");
        }
    }
}

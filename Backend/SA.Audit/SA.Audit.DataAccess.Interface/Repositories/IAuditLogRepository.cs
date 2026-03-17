namespace SA.Audit.DataAccess.Interface.Repositories;

using SA.Audit.Domain.Entities;

public interface IAuditLogRepository
{
    Task<(IReadOnlyList<AuditLog> Items, int Total)> ListAsync(
        Guid tenantId, int skip, int take,
        Guid? userId = null, string? operation = null, string? module = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null,
        string sort = "occurred_at", string order = "desc",
        CancellationToken ct = default);

    Task<IReadOnlyList<AuditLog>> ListForExportAsync(
        Guid tenantId, Guid? userId = null, string? operation = null,
        string? module = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default);

    Task<int> CountAsync(
        Guid tenantId, Guid? userId = null, string? operation = null,
        string? module = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default);

    Task AddAsync(AuditLog auditLog, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

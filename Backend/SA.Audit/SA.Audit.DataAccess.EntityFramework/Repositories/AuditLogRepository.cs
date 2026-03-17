using Microsoft.EntityFrameworkCore;
using SA.Audit.DataAccess.EntityFramework.Persistence;
using SA.Audit.DataAccess.Interface.Repositories;
using SA.Audit.Domain.Entities;

namespace SA.Audit.DataAccess.EntityFramework.Repositories;

public sealed class AuditLogRepository(AuditDbContext db) : IAuditLogRepository
{
    public async Task<(IReadOnlyList<AuditLog> Items, int Total)> ListAsync(
        Guid tenantId, int skip, int take,
        Guid? userId = null, string? operation = null, string? module = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null,
        string sort = "occurred_at", string order = "desc",
        CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(tenantId, userId, operation, module, from, to);

        var total = await query.CountAsync(ct);

        query = ApplySorting(query, sort, order);

        var items = await query
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<AuditLog>> ListForExportAsync(
        Guid tenantId, Guid? userId = null, string? operation = null,
        string? module = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(tenantId, userId, operation, module, from, to);

        return await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(10000)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        Guid tenantId, Guid? userId = null, string? operation = null,
        string? module = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(tenantId, userId, operation, module, from, to);
        return await query.AsNoTracking().CountAsync(ct);
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken ct = default)
    {
        await db.AuditLogs.AddAsync(auditLog, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    private IQueryable<AuditLog> BuildFilteredQuery(
        Guid tenantId, Guid? userId, string? operation,
        string? module, DateTimeOffset? from, DateTimeOffset? to)
    {
        var query = db.AuditLogs.Where(x => x.TenantId == tenantId);

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(operation))
            query = query.Where(x => x.Operation == operation);

        if (!string.IsNullOrWhiteSpace(module))
            query = query.Where(x => x.Module == module);

        if (from.HasValue)
            query = query.Where(x => x.OccurredAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.OccurredAt <= to.Value);

        return query;
    }

    private static IQueryable<AuditLog> ApplySorting(IQueryable<AuditLog> query, string sort, string order)
    {
        var isAsc = string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase);

        return sort.ToLowerInvariant() switch
        {
            "operation" => isAsc ? query.OrderBy(x => x.Operation) : query.OrderByDescending(x => x.Operation),
            "module" => isAsc ? query.OrderBy(x => x.Module) : query.OrderByDescending(x => x.Module),
            "user_name" => isAsc ? query.OrderBy(x => x.UserName) : query.OrderByDescending(x => x.UserName),
            _ => isAsc ? query.OrderBy(x => x.OccurredAt) : query.OrderByDescending(x => x.OccurredAt),
        };
    }
}

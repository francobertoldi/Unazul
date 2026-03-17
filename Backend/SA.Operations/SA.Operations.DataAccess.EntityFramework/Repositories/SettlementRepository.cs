using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class SettlementRepository(OperationsDbContext db) : ISettlementRepository
{
    public async Task<Settlement?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Settlements
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Totals)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Settlement> Items, int Total)> ListAsync(
        int skip,
        int take,
        Guid? tenantId = null,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        Guid? settledBy = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default)
    {
        var query = db.Settlements
            .AsNoTracking()
            .Include(x => x.Totals)
            .AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(x => x.TenantId == tenantId.Value);

        if (dateFrom.HasValue)
            query = query.Where(x => x.SettledAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(x => x.SettledAt <= dateTo.Value);

        if (settledBy.HasValue)
            query = query.Where(x => x.SettledBy == settledBy.Value);

        var total = await query.CountAsync(ct);

        var isDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLower() switch
        {
            "operation_count" => isDescending
                ? query.OrderByDescending(x => x.OperationCount)
                : query.OrderBy(x => x.OperationCount),
            _ => isDescending
                ? query.OrderByDescending(x => x.SettledAt)
                : query.OrderBy(x => x.SettledAt),
        };

        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Settlement settlement, CancellationToken ct = default)
    {
        await db.Settlements.AddAsync(settlement, ct);
    }

    public void Update(Settlement settlement)
    {
        db.Settlements.Update(settlement);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

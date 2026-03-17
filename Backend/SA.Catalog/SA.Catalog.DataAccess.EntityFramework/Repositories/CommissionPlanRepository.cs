using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class CommissionPlanRepository(CatalogDbContext db) : ICommissionPlanRepository
{
    public async Task<CommissionPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.CommissionPlans.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        return await db.CommissionPlans
            .AnyAsync(x => x.TenantId == tenantId && x.Code == code, ct);
    }

    public async Task<bool> ExistsByCodeExcludingAsync(Guid tenantId, string code, Guid excludeId, CancellationToken ct = default)
    {
        return await db.CommissionPlans
            .AnyAsync(x => x.TenantId == tenantId && x.Code == code && x.Id != excludeId, ct);
    }

    public async Task<(IReadOnlyList<CommissionPlan> Items, int Total)> ListAsync(int skip, int take, string? search, CancellationToken ct = default)
    {
        var query = db.CommissionPlans.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.Code, $"%{search}%") ||
                EF.Functions.ILike(x.Description, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.Code)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<int> CountAssignedPlansAsync(Guid commissionPlanId, CancellationToken ct = default)
    {
        return await db.ProductPlans
            .CountAsync(x => x.CommissionPlanId == commissionPlanId, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountAssignedPlansBatchAsync(IEnumerable<Guid> planIds, CancellationToken ct = default)
    {
        var idList = planIds.Distinct().ToList();
        return await db.ProductPlans
            .Where(x => x.CommissionPlanId.HasValue && idList.Contains(x.CommissionPlanId.Value))
            .GroupBy(x => x.CommissionPlanId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);
    }

    public async Task AddAsync(CommissionPlan plan, CancellationToken ct = default)
    {
        await db.CommissionPlans.AddAsync(plan, ct);
    }

    public void Update(CommissionPlan plan)
    {
        db.CommissionPlans.Update(plan);
    }

    public void Delete(CommissionPlan plan)
    {
        db.CommissionPlans.Remove(plan);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

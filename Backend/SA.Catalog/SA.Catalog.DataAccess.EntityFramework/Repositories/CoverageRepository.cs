using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class CoverageRepository(CatalogDbContext db) : ICoverageRepository
{
    public async Task<Coverage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Coverages.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(Guid planId, string name, CancellationToken ct = default)
    {
        return await db.Coverages
            .AnyAsync(x => x.PlanId == planId && x.Name == name, ct);
    }

    public async Task AddAsync(Coverage coverage, CancellationToken ct = default)
    {
        await db.Coverages.AddAsync(coverage, ct);
    }

    public async Task AddRangeAsync(IEnumerable<Coverage> coverages, CancellationToken ct = default)
    {
        await db.Coverages.AddRangeAsync(coverages, ct);
    }

    public void Update(Coverage coverage)
    {
        db.Coverages.Update(coverage);
    }

    public void Delete(Coverage coverage)
    {
        db.Coverages.Remove(coverage);
    }

    public async Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        var coverages = await db.Coverages
            .Where(x => x.PlanId == planId)
            .ToListAsync(ct);
        db.Coverages.RemoveRange(coverages);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

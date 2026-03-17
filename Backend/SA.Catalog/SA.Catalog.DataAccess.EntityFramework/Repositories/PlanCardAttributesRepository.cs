using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class PlanCardAttributesRepository(CatalogDbContext db) : IPlanCardAttributesRepository
{
    public async Task<PlanCardAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        return await db.PlanCardAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
    }

    public async Task AddAsync(PlanCardAttributes attributes, CancellationToken ct = default)
    {
        await db.PlanCardAttributes.AddAsync(attributes, ct);
    }

    public void Update(PlanCardAttributes attributes)
    {
        db.PlanCardAttributes.Update(attributes);
    }

    public void Delete(PlanCardAttributes attributes)
    {
        db.PlanCardAttributes.Remove(attributes);
    }

    public async Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        var entity = await db.PlanCardAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
        if (entity is not null) db.PlanCardAttributes.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

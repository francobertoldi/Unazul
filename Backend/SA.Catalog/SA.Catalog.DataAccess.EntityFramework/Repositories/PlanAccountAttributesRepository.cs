using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class PlanAccountAttributesRepository(CatalogDbContext db) : IPlanAccountAttributesRepository
{
    public async Task<PlanAccountAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        return await db.PlanAccountAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
    }

    public async Task AddAsync(PlanAccountAttributes attributes, CancellationToken ct = default)
    {
        await db.PlanAccountAttributes.AddAsync(attributes, ct);
    }

    public void Update(PlanAccountAttributes attributes)
    {
        db.PlanAccountAttributes.Update(attributes);
    }

    public void Delete(PlanAccountAttributes attributes)
    {
        db.PlanAccountAttributes.Remove(attributes);
    }

    public async Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        var entity = await db.PlanAccountAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
        if (entity is not null) db.PlanAccountAttributes.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

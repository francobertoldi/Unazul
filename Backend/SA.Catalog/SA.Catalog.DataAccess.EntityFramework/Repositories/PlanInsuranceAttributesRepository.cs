using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class PlanInsuranceAttributesRepository(CatalogDbContext db) : IPlanInsuranceAttributesRepository
{
    public async Task<PlanInsuranceAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        return await db.PlanInsuranceAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
    }

    public async Task AddAsync(PlanInsuranceAttributes attributes, CancellationToken ct = default)
    {
        await db.PlanInsuranceAttributes.AddAsync(attributes, ct);
    }

    public void Update(PlanInsuranceAttributes attributes)
    {
        db.PlanInsuranceAttributes.Update(attributes);
    }

    public void Delete(PlanInsuranceAttributes attributes)
    {
        db.PlanInsuranceAttributes.Remove(attributes);
    }

    public async Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        var entity = await db.PlanInsuranceAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
        if (entity is not null) db.PlanInsuranceAttributes.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

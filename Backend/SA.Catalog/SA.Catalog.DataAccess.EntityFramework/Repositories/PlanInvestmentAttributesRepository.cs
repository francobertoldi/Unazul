using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class PlanInvestmentAttributesRepository(CatalogDbContext db) : IPlanInvestmentAttributesRepository
{
    public async Task<PlanInvestmentAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        return await db.PlanInvestmentAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
    }

    public async Task AddAsync(PlanInvestmentAttributes attributes, CancellationToken ct = default)
    {
        await db.PlanInvestmentAttributes.AddAsync(attributes, ct);
    }

    public void Update(PlanInvestmentAttributes attributes)
    {
        db.PlanInvestmentAttributes.Update(attributes);
    }

    public void Delete(PlanInvestmentAttributes attributes)
    {
        db.PlanInvestmentAttributes.Remove(attributes);
    }

    public async Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        var entity = await db.PlanInvestmentAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
        if (entity is not null) db.PlanInvestmentAttributes.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

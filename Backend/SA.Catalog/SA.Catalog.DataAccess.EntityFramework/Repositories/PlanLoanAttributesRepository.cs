using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class PlanLoanAttributesRepository(CatalogDbContext db) : IPlanLoanAttributesRepository
{
    public async Task<PlanLoanAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        return await db.PlanLoanAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
    }

    public async Task AddAsync(PlanLoanAttributes attributes, CancellationToken ct = default)
    {
        await db.PlanLoanAttributes.AddAsync(attributes, ct);
    }

    public void Update(PlanLoanAttributes attributes)
    {
        db.PlanLoanAttributes.Update(attributes);
    }

    public void Delete(PlanLoanAttributes attributes)
    {
        db.PlanLoanAttributes.Remove(attributes);
    }

    public async Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        var entity = await db.PlanLoanAttributes.FirstOrDefaultAsync(x => x.PlanId == planId, ct);
        if (entity is not null) db.PlanLoanAttributes.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

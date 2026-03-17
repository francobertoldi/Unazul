using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class ProductPlanRepository(CatalogDbContext db) : IProductPlanRepository
{
    public async Task<ProductPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ProductPlans.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<ProductPlan?> GetByIdWithAttributesAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ProductPlans
            .AsNoTracking()
            .Include(x => x.CommissionPlan)
            .Include(x => x.LoanAttributes)
            .Include(x => x.InsuranceAttributes)
            .Include(x => x.AccountAttributes)
            .Include(x => x.CardAttributes)
            .Include(x => x.InvestmentAttributes)
            .Include(x => x.Coverages)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(ProductPlan plan, CancellationToken ct = default)
    {
        await db.ProductPlans.AddAsync(plan, ct);
    }

    public void Update(ProductPlan plan)
    {
        db.ProductPlans.Update(plan);
    }

    public void Delete(ProductPlan plan)
    {
        db.ProductPlans.Remove(plan);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

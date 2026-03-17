using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class ProductRequirementRepository(CatalogDbContext db) : IProductRequirementRepository
{
    public async Task<ProductRequirement?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ProductRequirements.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(ProductRequirement requirement, CancellationToken ct = default)
    {
        await db.ProductRequirements.AddAsync(requirement, ct);
    }

    public void Update(ProductRequirement requirement)
    {
        db.ProductRequirements.Update(requirement);
    }

    public void Delete(ProductRequirement requirement)
    {
        db.ProductRequirements.Remove(requirement);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        var requirements = await db.ProductRequirements
            .Where(x => x.ProductId == productId)
            .ToListAsync(ct);
        db.ProductRequirements.RemoveRange(requirements);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class ProductFamilyRepository(CatalogDbContext db) : IProductFamilyRepository
{
    public async Task<ProductFamily?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ProductFamilies.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        return await db.ProductFamilies
            .AnyAsync(x => x.TenantId == tenantId && x.Code == code, ct);
    }

    public async Task<(IReadOnlyList<ProductFamily> Items, int Total)> ListAsync(int skip, int take, string? search, CancellationToken ct = default)
    {
        var query = db.ProductFamilies.AsNoTracking().AsQueryable();

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

    public async Task<int> CountProductsAsync(Guid familyId, CancellationToken ct = default)
    {
        return await db.Products.CountAsync(x => x.FamilyId == familyId, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountProductsBatchAsync(IEnumerable<Guid> familyIds, CancellationToken ct = default)
    {
        var idList = familyIds.Distinct().ToList();
        return await db.Products
            .Where(x => idList.Contains(x.FamilyId))
            .GroupBy(x => x.FamilyId)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);
    }

    public async Task AddAsync(ProductFamily family, CancellationToken ct = default)
    {
        await db.ProductFamilies.AddAsync(family, ct);
    }

    public void Update(ProductFamily family)
    {
        db.ProductFamilies.Update(family);
    }

    public void Delete(ProductFamily family)
    {
        db.ProductFamilies.Remove(family);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

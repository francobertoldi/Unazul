using Microsoft.EntityFrameworkCore;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;

namespace SA.Catalog.DataAccess.EntityFramework.Repositories;

public sealed class ProductRepository(CatalogDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Products
            .AsNoTracking()
            .Include(x => x.Family)
            .Include(x => x.Plans)
                .ThenInclude(p => p.CommissionPlan)
            .Include(x => x.Plans)
                .ThenInclude(p => p.LoanAttributes)
            .Include(x => x.Plans)
                .ThenInclude(p => p.InsuranceAttributes)
            .Include(x => x.Plans)
                .ThenInclude(p => p.AccountAttributes)
            .Include(x => x.Plans)
                .ThenInclude(p => p.CardAttributes)
            .Include(x => x.Plans)
                .ThenInclude(p => p.InvestmentAttributes)
            .Include(x => x.Plans)
                .ThenInclude(p => p.Coverages)
            .Include(x => x.Requirements)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Product> Items, int Total)> ListAsync(
        int skip, int take, string? search, ProductStatus? status,
        Guid? familyId, Guid? entityId, bool excludeDeprecated,
        string sortBy, string sortDir, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking().Include(x => x.Family).AsQueryable();

        query = ApplyFilters(query, search, status, familyId, entityId, excludeDeprecated);

        var total = await query.CountAsync(ct);

        query = ApplySorting(query, sortBy, sortDir);

        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Product>> ListForExportAsync(
        string? search, ProductStatus? status, Guid? familyId,
        Guid? entityId, bool excludeDeprecated, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking().Include(x => x.Family).AsQueryable();

        query = ApplyFilters(query, search, status, familyId, entityId, excludeDeprecated);

        return await query.OrderBy(x => x.Name).ToListAsync(ct);
    }

    public async Task<int> CountForExportAsync(
        string? search, ProductStatus? status, Guid? familyId,
        Guid? entityId, bool excludeDeprecated, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        query = ApplyFilters(query, search, status, familyId, entityId, excludeDeprecated);

        return await query.CountAsync(ct);
    }

    public async Task<bool> HasPlansAsync(Guid productId, CancellationToken ct = default)
    {
        return await db.ProductPlans.AnyAsync(x => x.ProductId == productId, ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await db.Products.AddAsync(product, ct);
    }

    public void Update(Product product)
    {
        db.Products.Update(product);
    }

    public void Delete(Product product)
    {
        db.Products.Remove(product);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    private static IQueryable<Product> ApplyFilters(
        IQueryable<Product> query, string? search, ProductStatus? status,
        Guid? familyId, Guid? entityId, bool excludeDeprecated)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{search}%") ||
                EF.Functions.ILike(x.Code, $"%{search}%"));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (familyId.HasValue)
        {
            query = query.Where(x => x.FamilyId == familyId.Value);
        }

        if (entityId.HasValue)
        {
            query = query.Where(x => x.EntityId == entityId.Value);
        }

        if (excludeDeprecated)
        {
            query = query.Where(x => x.Status != ProductStatus.Deprecated);
        }

        return query;
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string sortDir)
    {
        var ascending = sortDir.Equals("asc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLower() switch
        {
            "code" => ascending ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "status" => ascending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
            "created_at" => ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
            _ => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name)
        };
    }
}

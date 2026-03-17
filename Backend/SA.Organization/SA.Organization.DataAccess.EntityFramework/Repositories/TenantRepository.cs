using Microsoft.EntityFrameworkCore;
using SA.Organization.DataAccess.EntityFramework.Persistence;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;

namespace SA.Organization.DataAccess.EntityFramework.Repositories;

public sealed class TenantRepository(OrganizationDbContext db) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<bool> ExistsByIdentifierAsync(string identifier, CancellationToken ct = default)
    {
        return await db.Tenants.AnyAsync(t => t.Identifier == identifier, ct);
    }

    public async Task<bool> ExistsByIdentifierExcludingAsync(string identifier, Guid excludeId, CancellationToken ct = default)
    {
        return await db.Tenants.AnyAsync(t => t.Identifier == identifier && t.Id != excludeId, ct);
    }

    public async Task<(IReadOnlyList<Tenant> Items, int Total)> ListAsync(
        int skip, int take, string? search, string? status, string? sort, string order, CancellationToken ct = default)
    {
        var query = db.Tenants.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(t =>
                EF.Functions.ILike(t.Name, $"%{s}%") ||
                EF.Functions.ILike(t.Identifier, $"%{s}%"));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TenantStatus>(status, true, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        var total = await query.CountAsync(ct);

        query = sort?.ToLower() switch
        {
            "identifier" => order == "desc" ? query.OrderByDescending(t => t.Identifier) : query.OrderBy(t => t.Identifier),
            "created_at" => order == "desc" ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => order == "desc" ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name)
        };

        var items = await query.Skip(skip).Take(take).ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Tenant>> ListForExportAsync(string? search, string? status, CancellationToken ct = default)
    {
        var query = db.Tenants.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(t =>
                EF.Functions.ILike(t.Name, $"%{s}%") ||
                EF.Functions.ILike(t.Identifier, $"%{s}%"));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TenantStatus>(status, true, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        return await query.OrderBy(t => t.Name).ToListAsync(ct);
    }

    public async Task<int> CountForExportAsync(string? search, string? status, CancellationToken ct = default)
    {
        var query = db.Tenants.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(t =>
                EF.Functions.ILike(t.Name, $"%{s}%") ||
                EF.Functions.ILike(t.Identifier, $"%{s}%"));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TenantStatus>(status, true, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        return await query.CountAsync(ct);
    }

    public async Task<int> CountEntitiesAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await db.Entities.CountAsync(e => e.TenantId == tenantId, ct);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await db.Tenants.AddAsync(tenant, ct);
    }

    public void Update(Tenant tenant)
    {
        db.Tenants.Update(tenant);
    }

    public Task DeleteAsync(Tenant tenant, CancellationToken ct = default)
    {
        db.Tenants.Remove(tenant);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

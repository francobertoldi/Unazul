using Microsoft.EntityFrameworkCore;
using SA.Organization.DataAccess.EntityFramework.Persistence;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using DomainEntity = SA.Organization.Domain.Entities.Entity;

namespace SA.Organization.DataAccess.EntityFramework.Repositories;

public sealed class EntityRepository(OrganizationDbContext db) : IEntityRepository
{
    public async Task<DomainEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Entities.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<DomainEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Entities
            .AsNoTracking()
            .Include(e => e.Channels)
            .Include(e => e.Branches)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<bool> ExistsByCuitAsync(Guid tenantId, string cuit, CancellationToken ct = default)
    {
        return await db.Entities.AnyAsync(e => e.TenantId == tenantId && e.Cuit == cuit, ct);
    }

    public async Task<bool> ExistsByCuitExcludingAsync(Guid tenantId, string cuit, Guid excludeId, CancellationToken ct = default)
    {
        return await db.Entities.AnyAsync(e => e.TenantId == tenantId && e.Cuit == cuit && e.Id != excludeId, ct);
    }

    public async Task<(IReadOnlyList<DomainEntity> Items, int Total)> ListAsync(
        int skip, int take, string? search, string? status, string? type, string? sort, string order, CancellationToken ct = default)
    {
        var query = db.Entities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.Name, $"%{s}%") ||
                EF.Functions.ILike(e.Cuit, $"%{s}%"));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EntityStatus>(status, true, out var statusEnum))
        {
            query = query.Where(e => e.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<EntityType>(type, true, out var typeEnum))
        {
            query = query.Where(e => e.Type == typeEnum);
        }

        var total = await query.CountAsync(ct);

        query = sort?.ToLower() switch
        {
            "cuit" => order == "desc" ? query.OrderByDescending(e => e.Cuit) : query.OrderBy(e => e.Cuit),
            "type" => order == "desc" ? query.OrderByDescending(e => e.Type) : query.OrderBy(e => e.Type),
            "created_at" => order == "desc" ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
            _ => order == "desc" ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name)
        };

        var items = await query.Skip(skip).Take(take).ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<DomainEntity>> ListForExportAsync(
        string? search, string? status, string? type, CancellationToken ct = default)
    {
        var query = db.Entities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.Name, $"%{s}%") ||
                EF.Functions.ILike(e.Cuit, $"%{s}%"));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EntityStatus>(status, true, out var statusEnum))
        {
            query = query.Where(e => e.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<EntityType>(type, true, out var typeEnum))
        {
            query = query.Where(e => e.Type == typeEnum);
        }

        return await query.OrderBy(e => e.Name).ToListAsync(ct);
    }

    public async Task<int> CountForExportAsync(
        string? search, string? status, string? type, CancellationToken ct = default)
    {
        var query = db.Entities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.Name, $"%{s}%") ||
                EF.Functions.ILike(e.Cuit, $"%{s}%"));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EntityStatus>(status, true, out var statusEnum))
        {
            query = query.Where(e => e.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<EntityType>(type, true, out var typeEnum))
        {
            query = query.Where(e => e.Type == typeEnum);
        }

        return await query.CountAsync(ct);
    }

    public async Task<bool> HasBranchesAsync(Guid entityId, CancellationToken ct = default)
    {
        return await db.Branches.AnyAsync(b => b.EntityId == entityId, ct);
    }

    public async Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await db.Entities.CountAsync(e => e.TenantId == tenantId, ct);
    }

    public async Task AddAsync(DomainEntity entity, CancellationToken ct = default)
    {
        await db.Entities.AddAsync(entity, ct);
    }

    public void Update(DomainEntity entity)
    {
        db.Entities.Update(entity);
    }

    public Task DeleteAsync(DomainEntity entity, CancellationToken ct = default)
    {
        db.Entities.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

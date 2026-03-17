using Microsoft.EntityFrameworkCore;
using SA.Identity.DataAccess.EntityFramework.Persistence;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Repositories;

public sealed class RoleRepository(IdentityDbContext db) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Roles.AsNoTracking()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken ct = default)
    {
        return await db.Roles
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == name, ct);
    }

    public async Task<(IReadOnlyList<Role> Items, int Total)> ListAsync(
        Guid tenantId,
        int skip,
        int take,
        string? search = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default)
    {
        var query = db.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .Where(r => r.TenantId == tenantId)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                EF.Functions.ILike(r.Name, $"%{search}%") ||
                EF.Functions.ILike(r.Description ?? "", $"%{search}%"));
        }

        var total = await query.CountAsync(ct);

        var isDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLower() switch
        {
            "created_at" => isDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            "updated_at" => isDescending ? query.OrderByDescending(r => r.UpdatedAt) : query.OrderBy(r => r.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
        };

        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Role role, CancellationToken ct = default)
    {
        await db.Roles.AddAsync(role, ct);
    }

    public void Update(Role role)
    {
        db.Roles.Update(role);
    }

    public void Delete(Role role)
    {
        db.Roles.Remove(role);
    }

    public async Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default)
    {
        var existing = await db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(ct);
        db.RolePermissions.RemoveRange(existing);

        var newPermissions = permissionIds.Select(pid => RolePermission.Create(roleId, pid));
        await db.RolePermissions.AddRangeAsync(newPermissions, ct);
    }

    public async Task<IReadOnlyList<Guid>> GetCurrentPermissionIdsAsync(Guid roleId, CancellationToken ct = default)
    {
        return await db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct);
    }

    public async Task<bool> HasAssignedUsersAsync(Guid roleId, CancellationToken ct = default)
    {
        return await db.UserRoles.AnyAsync(ur => ur.RoleId == roleId, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

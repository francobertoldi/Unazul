using Microsoft.EntityFrameworkCore;
using SA.Identity.DataAccess.EntityFramework.Persistence;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Enums;

namespace SA.Identity.DataAccess.EntityFramework.Repositories;

public sealed class UserRepository(IdentityDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Users.AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Assignments)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByUsernameAsync(Guid tenantId, string username, CancellationToken ct = default)
    {
        return await db.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username == username, ct);
    }

    public async Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default)
    {
        return await db.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email, ct);
    }

    public async Task<(IReadOnlyList<User> Items, int Total)> ListAsync(
        Guid tenantId,
        int skip,
        int take,
        string? search = null,
        UserStatus? status = null,
        Guid? roleId = null,
        Guid? entityId = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default)
    {
        var query = db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.TenantId == tenantId)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.Username, $"%{search}%") ||
                EF.Functions.ILike(u.Email, $"%{search}%") ||
                EF.Functions.ILike(u.FirstName, $"%{search}%") ||
                EF.Functions.ILike(u.LastName, $"%{search}%"));
        }

        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        if (roleId.HasValue)
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId.Value));
        }

        if (entityId.HasValue)
        {
            query = query.Where(u => u.EntityId == entityId.Value);
        }

        var total = await query.CountAsync(ct);

        var isDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLower() switch
        {
            "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "first_name" => isDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "last_name" => isDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            "status" => isDescending ? query.OrderByDescending(u => u.Status) : query.OrderBy(u => u.Status),
            "last_login" => isDescending ? query.OrderByDescending(u => u.LastLogin) : query.OrderBy(u => u.LastLogin),
            "created_at" => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => isDescending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
        };

        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await db.Users.AddAsync(user, ct);
    }

    public void Update(User user)
    {
        db.Users.Update(user);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task SetUserRolesAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken ct = default)
    {
        var existing = await db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(ct);
        db.UserRoles.RemoveRange(existing);

        var newRoles = roleIds.Select(roleId => UserRole.Create(userId, roleId));
        await db.UserRoles.AddRangeAsync(newRoles, ct);
    }

    public async Task SetUserAssignmentsAsync(Guid userId, IEnumerable<UserAssignment> assignments, CancellationToken ct = default)
    {
        var existing = await db.UserAssignments.Where(a => a.UserId == userId).ToListAsync(ct);
        db.UserAssignments.RemoveRange(existing);
        await db.UserAssignments.AddRangeAsync(assignments, ct);
    }

    public async Task<int> CountActiveSuperAdminsByTenantAsync(Guid tenantId, Guid excludeUserId, CancellationToken ct = default)
    {
        return await db.Users
            .Where(u => u.TenantId == tenantId
                && u.Id != excludeUserId
                && u.Status == UserStatus.Active
                && u.UserRoles.Any(ur => ur.Role.IsSystem && ur.Role.Name.Contains("Super Admin")))
            .CountAsync(ct);
    }
}

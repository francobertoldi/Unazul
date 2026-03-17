using Microsoft.EntityFrameworkCore;
using SA.Config.DataAccess.EntityFramework.Persistence;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Repositories;

/// <summary>
/// ParameterGroups is a GLOBAL table (no tenant_id). Queries bypass RLS by using
/// a raw DbConnection that does NOT set app.current_tenant. The TenantRlsInterceptor
/// only sets the session variable; it does not add a WHERE filter, so plain LINQ on
/// a table without a tenant_id column already works safely.  If the Postgres policy
/// is later broadened, switch to Database.GetDbConnection() with a fresh command.
/// </summary>
public sealed class ParameterGroupRepository(ConfigDbContext db) : IParameterGroupRepository
{
    public async Task<IReadOnlyList<ParameterGroup>> GetAllOrderedAsync(CancellationToken ct = default)
    {
        return await db.ParameterGroups.AsNoTracking()
            .OrderBy(g => g.Category)
            .ThenBy(g => g.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<ParameterGroup?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ParameterGroups.FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<ParameterGroup?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await db.ParameterGroups.FirstOrDefaultAsync(g => g.Code == code, ct);
    }

    public async Task<bool> HasParametersAsync(Guid groupId, CancellationToken ct = default)
    {
        // Checks across ALL tenants (global safety check before deleting a group).
        return await db.Parameters.AnyAsync(p => p.GroupId == groupId, ct);
    }

    public async Task AddAsync(ParameterGroup group, CancellationToken ct = default)
    {
        await db.ParameterGroups.AddAsync(group, ct);
    }

    public Task DeleteAsync(ParameterGroup group, CancellationToken ct = default)
    {
        db.ParameterGroups.Remove(group);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;
using SA.Organization.DataAccess.EntityFramework.Persistence;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.EntityFramework.Repositories;

public sealed class BranchRepository(OrganizationDbContext db) : IBranchRepository
{
    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Branches.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyList<Branch>> ListByEntityAsync(Guid entityId, CancellationToken ct = default)
    {
        return await db.Branches
            .AsNoTracking()
            .Where(b => b.EntityId == entityId)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        return await db.Branches.AnyAsync(b => b.TenantId == tenantId && b.Code == code, ct);
    }

    public async Task<bool> ExistsByCodeExcludingAsync(Guid tenantId, string code, Guid excludeId, CancellationToken ct = default)
    {
        return await db.Branches.AnyAsync(b => b.TenantId == tenantId && b.Code == code && b.Id != excludeId, ct);
    }

    public async Task<int> CountByEntityAsync(Guid entityId, CancellationToken ct = default)
    {
        return await db.Branches.CountAsync(b => b.EntityId == entityId, ct);
    }

    public async Task AddAsync(Branch branch, CancellationToken ct = default)
    {
        await db.Branches.AddAsync(branch, ct);
    }

    public void Update(Branch branch)
    {
        db.Branches.Update(branch);
    }

    public Task DeleteAsync(Branch branch, CancellationToken ct = default)
    {
        db.Branches.Remove(branch);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;
using SA.Config.DataAccess.EntityFramework.Persistence;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Repositories;

public sealed class ParameterRepository(ConfigDbContext db) : IParameterRepository
{
    public async Task<IReadOnlyList<Parameter>> GetByGroupIdAsync(
        Guid groupId,
        string? parentKey = null,
        CancellationToken ct = default)
    {
        var query = db.Parameters.AsNoTracking()
            .Include(p => p.Options.OrderBy(o => o.SortOrder))
            .Where(p => p.GroupId == groupId);

        if (parentKey is not null)
        {
            query = query.Where(p => p.ParentKey == parentKey);
        }
        else
        {
            query = query.Where(p => p.ParentKey == null);
        }

        return await query
            .OrderBy(p => p.Key)
            .ToListAsync(ct);
    }

    public async Task<Parameter?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Parameters
            .Include(p => p.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<bool> ExistsByKeyAsync(Guid tenantId, Guid groupId, string key, CancellationToken ct = default)
    {
        return await db.Parameters
            .AnyAsync(p => p.TenantId == tenantId && p.GroupId == groupId && p.Key == key, ct);
    }

    public async Task AddAsync(Parameter parameter, CancellationToken ct = default)
    {
        await db.Parameters.AddAsync(parameter, ct);
    }

    public void Update(Parameter parameter)
    {
        db.Parameters.Update(parameter);
    }

    public Task DeleteAsync(Parameter parameter, CancellationToken ct = default)
    {
        db.Parameters.Remove(parameter);
        return Task.CompletedTask;
    }

    public async Task ReplaceOptionsAsync(Guid parameterId, IEnumerable<ParameterOption> newOptions, CancellationToken ct = default)
    {
        var existing = await db.ParameterOptions
            .Where(o => o.ParameterId == parameterId)
            .ToListAsync(ct);

        db.ParameterOptions.RemoveRange(existing);
        await db.ParameterOptions.AddRangeAsync(newOptions, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

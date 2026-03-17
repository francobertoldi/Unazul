using Microsoft.EntityFrameworkCore;
using SA.Config.DataAccess.EntityFramework.Persistence;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.EntityFramework.Repositories;

public sealed class ExternalServiceRepository(ConfigDbContext db) : IExternalServiceRepository
{
    public async Task<IReadOnlyList<ExternalService>> GetAllByTenantAsync(CancellationToken ct = default)
    {
        return await db.ExternalServices.AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<ExternalService?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ExternalServices.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<ExternalService?> GetByIdWithAuthConfigsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ExternalServices
            .Include(s => s.AuthConfigs)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = db.ExternalServices
            .Where(s => s.TenantId == tenantId && s.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(ExternalService service, CancellationToken ct = default)
    {
        await db.ExternalServices.AddAsync(service, ct);
    }

    public void Update(ExternalService service)
    {
        db.ExternalServices.Update(service);
    }

    public async Task ReplaceAuthConfigsAsync(Guid serviceId, IEnumerable<ServiceAuthConfig> configs, CancellationToken ct = default)
    {
        var existing = await db.ServiceAuthConfigs
            .Where(c => c.ServiceId == serviceId)
            .ToListAsync(ct);

        db.ServiceAuthConfigs.RemoveRange(existing);
        await db.ServiceAuthConfigs.AddRangeAsync(configs, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

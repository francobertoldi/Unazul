using Microsoft.EntityFrameworkCore;
using SA.Organization.DataAccess.EntityFramework.Persistence;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.EntityFramework.Repositories;

public sealed class EntityChannelRepository(OrganizationDbContext db) : IEntityChannelRepository
{
    public async Task<IReadOnlyList<EntityChannel>> GetByEntityIdAsync(Guid entityId, CancellationToken ct = default)
    {
        return await db.EntityChannels
            .AsNoTracking()
            .Where(c => c.EntityId == entityId)
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<EntityChannel> channels, CancellationToken ct = default)
    {
        await db.EntityChannels.AddRangeAsync(channels, ct);
    }

    public void RemoveRange(IEnumerable<EntityChannel> channels)
    {
        db.EntityChannels.RemoveRange(channels);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

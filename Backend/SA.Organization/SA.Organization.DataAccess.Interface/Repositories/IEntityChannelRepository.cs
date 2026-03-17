using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.Interface.Repositories;

public interface IEntityChannelRepository
{
    Task<IReadOnlyList<EntityChannel>> GetByEntityIdAsync(Guid entityId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<EntityChannel> channels, CancellationToken ct = default);
    void RemoveRange(IEnumerable<EntityChannel> channels);
    Task SaveChangesAsync(CancellationToken ct = default);
}

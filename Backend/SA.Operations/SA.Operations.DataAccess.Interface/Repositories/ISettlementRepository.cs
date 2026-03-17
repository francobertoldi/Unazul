using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.Interface.Repositories;

public interface ISettlementRepository
{
    Task<Settlement?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Settlement> Items, int Total)> ListAsync(
        int skip,
        int take,
        Guid? tenantId = null,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        Guid? settledBy = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default);
    Task AddAsync(Settlement settlement, CancellationToken ct = default);
    void Update(Settlement settlement);
    Task SaveChangesAsync(CancellationToken ct = default);
}

using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface ICommissionPlanRepository
{
    Task<CommissionPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<bool> ExistsByCodeExcludingAsync(Guid tenantId, string code, Guid excludeId, CancellationToken ct = default);
    Task<(IReadOnlyList<CommissionPlan> Items, int Total)> ListAsync(int skip, int take, string? search, CancellationToken ct = default);
    Task<int> CountAssignedPlansAsync(Guid commissionPlanId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, int>> CountAssignedPlansBatchAsync(IEnumerable<Guid> planIds, CancellationToken ct = default);
    Task AddAsync(CommissionPlan plan, CancellationToken ct = default);
    void Update(CommissionPlan plan);
    void Delete(CommissionPlan plan);
    Task SaveChangesAsync(CancellationToken ct = default);
}

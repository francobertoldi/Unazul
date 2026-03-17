using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.Interface.Repositories;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Branch>> ListByEntityAsync(Guid entityId, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<bool> ExistsByCodeExcludingAsync(Guid tenantId, string code, Guid excludeId, CancellationToken ct = default);
    Task<int> CountByEntityAsync(Guid entityId, CancellationToken ct = default);
    Task AddAsync(Branch branch, CancellationToken ct = default);
    void Update(Branch branch);
    Task DeleteAsync(Branch branch, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

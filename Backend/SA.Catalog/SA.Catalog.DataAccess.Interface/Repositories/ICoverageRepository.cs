using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface ICoverageRepository
{
    Task<Coverage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(Guid planId, string name, CancellationToken ct = default);
    Task AddAsync(Coverage coverage, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Coverage> coverages, CancellationToken ct = default);
    void Update(Coverage coverage);
    void Delete(Coverage coverage);
    Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

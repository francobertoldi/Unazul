using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IPlanCardAttributesRepository
{
    Task<PlanCardAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task AddAsync(PlanCardAttributes attributes, CancellationToken ct = default);
    void Update(PlanCardAttributes attributes);
    void Delete(PlanCardAttributes attributes);
    Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

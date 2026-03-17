using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IPlanAccountAttributesRepository
{
    Task<PlanAccountAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task AddAsync(PlanAccountAttributes attributes, CancellationToken ct = default);
    void Update(PlanAccountAttributes attributes);
    void Delete(PlanAccountAttributes attributes);
    Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

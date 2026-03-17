using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IPlanInsuranceAttributesRepository
{
    Task<PlanInsuranceAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task AddAsync(PlanInsuranceAttributes attributes, CancellationToken ct = default);
    void Update(PlanInsuranceAttributes attributes);
    void Delete(PlanInsuranceAttributes attributes);
    Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

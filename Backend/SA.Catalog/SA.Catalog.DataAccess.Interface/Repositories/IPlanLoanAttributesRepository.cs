using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IPlanLoanAttributesRepository
{
    Task<PlanLoanAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task AddAsync(PlanLoanAttributes attributes, CancellationToken ct = default);
    void Update(PlanLoanAttributes attributes);
    void Delete(PlanLoanAttributes attributes);
    Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

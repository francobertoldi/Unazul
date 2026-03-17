using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IPlanInvestmentAttributesRepository
{
    Task<PlanInvestmentAttributes?> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task AddAsync(PlanInvestmentAttributes attributes, CancellationToken ct = default);
    void Update(PlanInvestmentAttributes attributes);
    void Delete(PlanInvestmentAttributes attributes);
    Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

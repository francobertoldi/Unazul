using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IProductPlanRepository
{
    Task<ProductPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductPlan?> GetByIdWithAttributesAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ProductPlan plan, CancellationToken ct = default);
    void Update(ProductPlan plan);
    void Delete(ProductPlan plan);
    Task SaveChangesAsync(CancellationToken ct = default);
}

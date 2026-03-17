using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IProductRequirementRepository
{
    Task<ProductRequirement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ProductRequirement requirement, CancellationToken ct = default);
    void Update(ProductRequirement requirement);
    void Delete(ProductRequirement requirement);
    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IProductFamilyRepository
{
    Task<ProductFamily?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<(IReadOnlyList<ProductFamily> Items, int Total)> ListAsync(int skip, int take, string? search, CancellationToken ct = default);
    Task<int> CountProductsAsync(Guid familyId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, int>> CountProductsBatchAsync(IEnumerable<Guid> familyIds, CancellationToken ct = default);
    Task AddAsync(ProductFamily family, CancellationToken ct = default);
    void Update(ProductFamily family);
    void Delete(ProductFamily family);
    Task SaveChangesAsync(CancellationToken ct = default);
}

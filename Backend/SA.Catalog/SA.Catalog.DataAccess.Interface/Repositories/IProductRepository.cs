using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;

namespace SA.Catalog.DataAccess.Interface.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int Total)> ListAsync(int skip, int take, string? search, ProductStatus? status, Guid? familyId, Guid? entityId, bool excludeDeprecated, string sortBy, string sortDir, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> ListForExportAsync(string? search, ProductStatus? status, Guid? familyId, Guid? entityId, bool excludeDeprecated, CancellationToken ct = default);
    Task<int> CountForExportAsync(string? search, ProductStatus? status, Guid? familyId, Guid? entityId, bool excludeDeprecated, CancellationToken ct = default);
    Task<bool> HasPlansAsync(Guid productId, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Delete(Product product);
    Task SaveChangesAsync(CancellationToken ct = default);
}

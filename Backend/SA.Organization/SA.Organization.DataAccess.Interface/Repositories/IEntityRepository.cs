using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.Interface.Repositories;

public interface IEntityRepository
{
    Task<Entity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Entity?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCuitAsync(Guid tenantId, string cuit, CancellationToken ct = default);
    Task<bool> ExistsByCuitExcludingAsync(Guid tenantId, string cuit, Guid excludeId, CancellationToken ct = default);
    Task<(IReadOnlyList<Entity> Items, int Total)> ListAsync(int skip, int take, string? search, string? status, string? type, string? sort, string order, CancellationToken ct = default);
    Task<IReadOnlyList<Entity>> ListForExportAsync(string? search, string? status, string? type, CancellationToken ct = default);
    Task<int> CountForExportAsync(string? search, string? status, string? type, CancellationToken ct = default);
    Task<bool> HasBranchesAsync(Guid entityId, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Entity entity, CancellationToken ct = default);
    void Update(Entity entity);
    Task DeleteAsync(Entity entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

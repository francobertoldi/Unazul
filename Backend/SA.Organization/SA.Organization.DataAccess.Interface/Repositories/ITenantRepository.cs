using SA.Organization.Domain.Entities;

namespace SA.Organization.DataAccess.Interface.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByIdentifierAsync(string identifier, CancellationToken ct = default);
    Task<bool> ExistsByIdentifierExcludingAsync(string identifier, Guid excludeId, CancellationToken ct = default);
    Task<(IReadOnlyList<Tenant> Items, int Total)> ListAsync(int skip, int take, string? search, string? status, string? sort, string order, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> ListForExportAsync(string? search, string? status, CancellationToken ct = default);
    Task<int> CountForExportAsync(string? search, string? status, CancellationToken ct = default);
    Task<int> CountEntitiesAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    void Update(Tenant tenant);
    Task DeleteAsync(Tenant tenant, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

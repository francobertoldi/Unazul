using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.Interface.Repositories;

public interface IExternalServiceRepository
{
    Task<IReadOnlyList<ExternalService>> GetAllByTenantAsync(CancellationToken ct = default);
    Task<ExternalService?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExternalService?> GetByIdWithAuthConfigsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default);
    Task AddAsync(ExternalService service, CancellationToken ct = default);
    void Update(ExternalService service);
    Task ReplaceAuthConfigsAsync(Guid serviceId, IEnumerable<ServiceAuthConfig> configs, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

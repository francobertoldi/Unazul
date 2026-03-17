using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.Interface.Repositories;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetEffectivePermissionsByUserIdAsync(Guid userId, CancellationToken ct = default);
}

using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.Interface.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken ct = default);
    Task<(IReadOnlyList<Role> Items, int Total)> ListAsync(
        Guid tenantId,
        int skip,
        int take,
        string? search = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default);
    Task AddAsync(Role role, CancellationToken ct = default);
    void Update(Role role);
    void Delete(Role role);
    Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetCurrentPermissionIdsAsync(Guid roleId, CancellationToken ct = default);
    Task<bool> HasAssignedUsersAsync(Guid roleId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

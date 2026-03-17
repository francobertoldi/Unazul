using SA.Identity.Domain.Entities;
using Shared.Contract.Enums;

namespace SA.Identity.DataAccess.Interface.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(Guid tenantId, string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int Total)> ListAsync(
        Guid tenantId,
        int skip,
        int take,
        string? search = null,
        UserStatus? status = null,
        Guid? roleId = null,
        Guid? entityId = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task SetUserRolesAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken ct = default);
    Task SetUserAssignmentsAsync(Guid userId, IEnumerable<UserAssignment> assignments, CancellationToken ct = default);
    Task<int> CountActiveSuperAdminsByTenantAsync(Guid tenantId, Guid excludeUserId, CancellationToken ct = default);
}

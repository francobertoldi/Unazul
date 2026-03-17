using Mediator;
using SA.Identity.Application.Dtos.Users;
using SA.Identity.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Queries.Users;

public sealed class GetUserDetailQueryHandler(
    IUserRepository userRepository,
    IPermissionRepository permissionRepository) : IQueryHandler<GetUserDetailQuery, UserDetailDto>
{
    public async ValueTask<UserDetailDto> Handle(GetUserDetailQuery query, CancellationToken ct)
    {
        var user = await userRepository.GetByIdWithRolesAsync(query.UserId, ct)
            ?? throw new NotFoundException("USERS_NOT_FOUND", "Usuario no encontrado.");

        var roles = user.UserRoles
            .Select(ur => new UserDetailRoleDto(ur.Role.Id, ur.Role.Name))
            .ToList();

        var assignments = user.Assignments
            .Select(a => new UserDetailAssignmentDto(a.Id, a.ScopeType, a.ScopeId, a.ScopeName))
            .ToList();

        // Get all effective permissions (DISTINCT, ordered by module then code)
        var effectivePermissions = await permissionRepository.GetEffectivePermissionsByUserIdAsync(user.Id, ct);
        var permissionDtos = effectivePermissions
            .Select(p => new UserDetailPermissionDto(p.Id, p.Module, p.Action, p.Code, p.Description))
            .ToList();

        return new UserDetailDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.EntityId,
            user.EntityName,
            user.Status,
            user.FailedLoginAttempts,
            user.LastLogin,
            user.Avatar,
            user.CreatedAt,
            user.UpdatedAt,
            user.CreatedBy,
            user.UpdatedBy,
            roles,
            assignments,
            permissionDtos);
    }
}

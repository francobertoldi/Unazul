using SA.Identity.Domain.Entities;

namespace SA.Identity.Application.Dtos.Users;

internal static class UserMapper
{
    internal static UserDetailDto ToDetailDto(User user)
    {
        var roles = user.UserRoles
            .Select(ur => new UserDetailRoleDto(ur.Role.Id, ur.Role.Name))
            .ToList();

        var assignments = user.Assignments
            .Select(a => new UserDetailAssignmentDto(a.Id, a.ScopeType, a.ScopeId, a.ScopeName))
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
            assignments);
    }
}

using SA.Identity.Domain.Entities;

namespace SA.Identity.Application.Dtos.Roles;

internal static class RoleMapper
{
    internal static RoleDetailDto ToDetailDto(Role role)
    {
        var permissions = role.RolePermissions
            .Select(rp => new RolePermissionDto(
                rp.Permission.Id,
                rp.Permission.Module,
                rp.Permission.Action,
                rp.Permission.Code,
                rp.Permission.Description))
            .ToList();

        return new RoleDetailDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem,
            role.CreatedAt,
            role.UpdatedAt,
            permissions);
    }
}

using Mediator;
using SA.Identity.Application.Dtos.Roles;

namespace SA.Identity.Application.Commands.Roles;

public readonly record struct UpdateRoleCommand(
    Guid RoleId,
    Guid TenantId,
    string Name,
    string? Description,
    Guid[] PermissionIds,
    Guid UpdatedBy) : ICommand<RoleDetailDto>;

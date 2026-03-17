using Mediator;
using SA.Identity.Application.Dtos.Roles;

namespace SA.Identity.Application.Commands.Roles;

public readonly record struct CreateRoleCommand(
    Guid TenantId,
    string Name,
    string? Description,
    Guid[] PermissionIds,
    Guid CreatedBy) : ICommand<RoleDetailDto>;

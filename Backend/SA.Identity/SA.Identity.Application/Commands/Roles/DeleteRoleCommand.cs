using Mediator;

namespace SA.Identity.Application.Commands.Roles;

public readonly record struct DeleteRoleCommand(
    Guid RoleId,
    Guid TenantId,
    Guid DeletedBy) : ICommand;

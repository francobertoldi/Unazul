using Mediator;
using SA.Identity.Application.Dtos.Roles;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Roles;

public sealed class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<UpdateRoleCommand, RoleDetailDto>
{
    // Critical permissions that Super Admin must always have
    private static readonly HashSet<string> CriticalPermissionCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "p_roles_list",
        "p_roles_create",
        "p_roles_edit",
        "p_roles_delete",
        "p_users_list",
        "p_users_create",
        "p_users_edit"
    };

    public async ValueTask<RoleDetailDto> Handle(UpdateRoleCommand command, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdWithPermissionsAsync(command.RoleId, ct)
            ?? throw new NotFoundException("ROLE_NOT_FOUND", "Rol no encontrado.");

        // Validate permission_ids must not be empty
        if (command.PermissionIds.Length == 0)
        {
            throw new ValidationException("ROLE_PERMISSIONS_EMPTY", "El rol debe tener al menos un permiso asignado.");
        }

        // Deduplicate permission_ids
        var uniquePermissionIds = command.PermissionIds.Distinct().ToArray();

        // Super Admin lockout protection
        if (role.IsSystem && role.Name.Contains("Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            // Validate resulting permissions include all critical ones
            var newPermissions = await permissionRepository.GetByIdsAsync(uniquePermissionIds, ct);
            var newPermissionCodes = newPermissions.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingCritical = CriticalPermissionCodes.Except(newPermissionCodes).ToList();
            if (missingCritical.Count > 0)
            {
                throw new ValidationException(
                    "ROLE_SUPER_ADMIN_MISSING_CRITICAL_PERMISSIONS",
                    $"El rol Super Admin debe mantener los permisos críticos: {string.Join(", ", missingCritical)}");
            }
        }
        else
        {
            // Non-system roles cannot be edited if system
            if (role.IsSystem)
            {
                throw new ValidationException("ROLE_SYSTEM_READONLY", "No se puede modificar un rol de sistema.");
            }
        }

        var existingByName = await roleRepository.GetByNameAsync(command.TenantId, command.Name, ct);
        if (existingByName is not null && existingByName.Id != command.RoleId)
        {
            throw new ConflictException("ROLE_DUPLICATE_NAME", "Ya existe un rol con ese nombre.");
        }

        // Calculate permission diff before updating
        var currentPermissionIds = await roleRepository.GetCurrentPermissionIdsAsync(command.RoleId, ct);
        var newPermissionIds = uniquePermissionIds.ToHashSet();
        var currentSet = currentPermissionIds.ToHashSet();

        var addedPermissionIds = newPermissionIds.Except(currentSet).ToArray();
        var removedPermissionIds = currentSet.Except(newPermissionIds).ToArray();

        role.Update(command.Name, command.Description, command.UpdatedBy);
        roleRepository.Update(role);

        await roleRepository.SetRolePermissionsAsync(command.RoleId, uniquePermissionIds, ct);
        await roleRepository.SaveChangesAsync(ct);

        // Publish RoleUpdatedEvent with diff
        if (addedPermissionIds.Length > 0 || removedPermissionIds.Length > 0)
        {
            await eventPublisher.PublishAsync(new RoleUpdatedEvent(
                TenantId: command.TenantId,
                RoleId: command.RoleId,
                RoleName: command.Name,
                AddedPermissionIds: addedPermissionIds,
                RemovedPermissionIds: removedPermissionIds,
                UpdatedBy: command.UpdatedBy,
                OccurredAt: DateTimeOffset.UtcNow,
                CorrelationId: Guid.CreateVersion7()), ct);
        }

        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: command.TenantId,
            UserId: command.UpdatedBy,
            UserName: string.Empty,
            Operation: "WRITE",
            Module: "identity",
            Action: "role_updated",
            Detail: $"Role {command.Name} updated. Added: {addedPermissionIds.Length}, Removed: {removedPermissionIds.Length}",
            IpAddress: null,
            EntityType: "Role",
            EntityId: command.RoleId,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        var updatedRole = await roleRepository.GetByIdWithPermissionsAsync(command.RoleId, ct);

        return RoleMapper.ToDetailDto(updatedRole!);
    }
}

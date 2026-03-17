using Mediator;
using SA.Identity.Application.Dtos.Roles;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Roles;

public sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<CreateRoleCommand, RoleDetailDto>
{
    public async ValueTask<RoleDetailDto> Handle(CreateRoleCommand command, CancellationToken ct)
    {
        // Validate permission_ids must not be empty
        if (command.PermissionIds.Length == 0)
        {
            throw new ValidationException("ROLE_PERMISSIONS_EMPTY", "El rol debe tener al menos un permiso asignado.");
        }

        // Deduplicate permission_ids
        var uniquePermissionIds = command.PermissionIds.Distinct().ToArray();

        // Validate all permission_ids exist
        var existingPermissions = await permissionRepository.GetByIdsAsync(uniquePermissionIds, ct);
        if (existingPermissions.Count != uniquePermissionIds.Length)
        {
            throw new NotFoundException("ROLE_PERMISSION_NOT_FOUND", "Uno o más permisos no fueron encontrados.");
        }

        var existingRole = await roleRepository.GetByNameAsync(command.TenantId, command.Name, ct);
        if (existingRole is not null)
        {
            throw new ConflictException("ROLE_DUPLICATE_NAME", "Ya existe un rol con ese nombre.");
        }

        var role = Role.Create(
            command.TenantId,
            command.Name,
            command.Description,
            command.CreatedBy);

        await roleRepository.AddAsync(role, ct);
        await roleRepository.SaveChangesAsync(ct);

        await roleRepository.SetRolePermissionsAsync(role.Id, uniquePermissionIds, ct);
        await roleRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: command.TenantId,
            UserId: command.CreatedBy,
            UserName: string.Empty,
            Operation: "WRITE",
            Module: "identity",
            Action: "role_created",
            Detail: $"Role {command.Name} created",
            IpAddress: null,
            EntityType: "Role",
            EntityId: role.Id,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        var fullRole = await roleRepository.GetByIdWithPermissionsAsync(role.Id, ct);

        return RoleMapper.ToDetailDto(fullRole!);
    }
}

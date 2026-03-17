using Mediator;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Roles;

public sealed class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<DeleteRoleCommand>
{
    public async ValueTask<Unit> Handle(DeleteRoleCommand command, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdWithPermissionsAsync(command.RoleId, ct)
            ?? throw new NotFoundException("ROLE_NOT_FOUND", "Rol no encontrado.");

        if (role.IsSystem)
        {
            throw new ValidationException("ROLE_SYSTEM_READONLY", "No se puede eliminar un rol de sistema.");
        }

        var hasUsers = await roleRepository.HasAssignedUsersAsync(command.RoleId, ct);
        if (hasUsers)
        {
            throw new ConflictException("ROLE_HAS_USERS", "No se puede eliminar un rol que tiene usuarios asignados.");
        }

        // Physical delete: role_permissions then role
        await roleRepository.SetRolePermissionsAsync(command.RoleId, [], ct);
        roleRepository.Delete(role);
        await roleRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: command.TenantId,
            UserId: command.DeletedBy,
            UserName: string.Empty,
            Operation: "DELETE",
            Module: "identity",
            Action: "role_deleted",
            Detail: $"Role {role.Name} deleted",
            IpAddress: null,
            EntityType: "Role",
            EntityId: command.RoleId,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return Unit.Value;
    }
}

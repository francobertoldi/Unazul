using Mediator;
using SA.Identity.Application.Dtos.Users;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Users;

public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<UpdateUserCommand, UserDetailDto>
{
    public async ValueTask<UserDetailDto> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByIdWithRolesAsync(command.UserId, ct)
            ?? throw new NotFoundException("USERS_NOT_FOUND", "Usuario no encontrado.");

        var existingByEmail = await userRepository.GetByEmailAsync(command.TenantId, command.Email, ct);
        if (existingByEmail is not null && existingByEmail.Id != command.UserId)
        {
            throw new ConflictException("USERS_EMAIL_IN_USE", "El email ya está en uso.");
        }

        // If changing status to locked/inactive, check if user is last active Super Admin
        if (command.Status != UserStatus.Active && user.Status == UserStatus.Active)
        {
            var isSuperAdmin = user.UserRoles.Any(ur => ur.Role.IsSystem && ur.Role.Name.Contains("Super Admin"));
            if (isSuperAdmin)
            {
                var otherSuperAdminCount = await userRepository.CountActiveSuperAdminsByTenantAsync(
                    command.TenantId, command.UserId, ct);
                if (otherSuperAdminCount < 1)
                {
                    throw new ConflictException("USERS_LAST_SUPER_ADMIN", "No se puede desactivar al último Super Admin.");
                }
            }
        }

        user.Update(
            command.Email,
            command.FirstName,
            command.LastName,
            command.EntityId,
            command.EntityName,
            command.Status,
            command.Avatar,
            command.UpdatedBy);

        userRepository.Update(user);

        await userRepository.SetUserRolesAsync(user.Id, command.RoleIds, ct);

        var assignments = command.Assignments
            .Select(a => UserAssignment.Create(user.Id, a.ScopeType, a.ScopeId, a.ScopeName))
            .ToList();
        await userRepository.SetUserAssignmentsAsync(user.Id, assignments, ct);

        await userRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: command.TenantId,
            UserId: command.UpdatedBy,
            UserName: user.Username,
            Operation: "WRITE",
            Module: "identity",
            Action: "user_updated",
            Detail: $"User {user.Username} updated",
            IpAddress: null,
            EntityType: "User",
            EntityId: user.Id,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        var updatedUser = await userRepository.GetByIdWithRolesAsync(user.Id, ct);

        return UserMapper.ToDetailDto(updatedUser!);
    }
}

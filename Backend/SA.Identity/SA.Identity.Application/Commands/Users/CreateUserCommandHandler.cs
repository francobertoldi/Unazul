using System.Text.RegularExpressions;
using Mediator;
using SA.Identity.Application.Dtos.Users;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Users;

public sealed partial class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<CreateUserCommand, UserDetailDto>
{
    [GeneratedRegex(@"^[a-zA-Z0-9._\-]{3,30}$")]
    private static partial Regex UsernameRegex();

    public async ValueTask<UserDetailDto> Handle(CreateUserCommand command, CancellationToken ct)
    {
        if (!UsernameRegex().IsMatch(command.Username))
        {
            throw new ValidationException("USERS_INVALID_USERNAME", "El nombre de usuario debe tener entre 3 y 30 caracteres alfanuméricos.");
        }

        var existingByUsername = await userRepository.GetByUsernameAsync(command.TenantId, command.Username, ct);
        if (existingByUsername is not null)
        {
            throw new ConflictException("USERS_USERNAME_IN_USE", "El nombre de usuario ya está en uso.");
        }

        var existingByEmail = await userRepository.GetByEmailAsync(command.TenantId, command.Email, ct);
        if (existingByEmail is not null)
        {
            throw new ConflictException("USERS_EMAIL_IN_USE", "El email ya está en uso.");
        }

        var hashedPassword = passwordService.Hash(command.Password);

        var user = User.Create(
            command.TenantId,
            command.Username,
            hashedPassword,
            command.Email,
            command.FirstName,
            command.LastName,
            command.EntityId,
            command.EntityName,
            command.CreatedBy);

        await userRepository.AddAsync(user, ct);
        await userRepository.SaveChangesAsync(ct);

        if (command.RoleIds.Length > 0)
        {
            await userRepository.SetUserRolesAsync(user.Id, command.RoleIds, ct);
        }

        if (command.Assignments.Length > 0)
        {
            var assignments = command.Assignments
                .Select(a => UserAssignment.Create(user.Id, a.ScopeType, a.ScopeId, a.ScopeName))
                .ToList();
            await userRepository.SetUserAssignmentsAsync(user.Id, assignments, ct);
        }

        await userRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: command.TenantId,
            UserId: command.CreatedBy,
            UserName: command.Username,
            Operation: "WRITE",
            Module: "identity",
            Action: "user_created",
            Detail: $"User {command.Username} created",
            IpAddress: null,
            EntityType: "User",
            EntityId: user.Id,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        var fullUser = await userRepository.GetByIdWithRolesAsync(user.Id, ct);

        return UserMapper.ToDetailDto(fullUser!);
    }
}

using Mediator;
using SA.Identity.Application.Dtos.Users;

namespace SA.Identity.Application.Commands.Users;

public readonly record struct CreateUserCommand(
    Guid TenantId,
    string Username,
    string Password,
    string Email,
    string FirstName,
    string LastName,
    Guid? EntityId,
    string? EntityName,
    Guid[] RoleIds,
    CreateUserAssignmentInput[] Assignments,
    Guid CreatedBy) : ICommand<UserDetailDto>;

public sealed record CreateUserAssignmentInput(string ScopeType, Guid ScopeId, string ScopeName);

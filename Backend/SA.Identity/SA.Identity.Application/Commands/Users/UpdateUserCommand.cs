using Mediator;
using SA.Identity.Application.Dtos.Users;
using Shared.Contract.Enums;

namespace SA.Identity.Application.Commands.Users;

public readonly record struct UpdateUserCommand(
    Guid UserId,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    Guid? EntityId,
    string? EntityName,
    UserStatus Status,
    string? Avatar,
    Guid[] RoleIds,
    UpdateUserAssignmentInput[] Assignments,
    Guid UpdatedBy) : ICommand<UserDetailDto>;

public sealed record UpdateUserAssignmentInput(string ScopeType, Guid ScopeId, string ScopeName);

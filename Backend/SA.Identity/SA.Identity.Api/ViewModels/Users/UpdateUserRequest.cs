using Shared.Contract.Enums;

namespace SA.Identity.Api.ViewModels.Users;

public sealed record UpdateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    Guid? EntityId,
    string? EntityName,
    UserStatus Status,
    string? Avatar,
    Guid[] RoleIds,
    UpdateUserAssignmentRequest[] Assignments);

public sealed record UpdateUserAssignmentRequest(string ScopeType, Guid ScopeId, string ScopeName);

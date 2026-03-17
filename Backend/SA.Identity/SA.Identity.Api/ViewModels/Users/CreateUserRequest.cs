namespace SA.Identity.Api.ViewModels.Users;

public sealed record CreateUserRequest(
    string Username,
    string Password,
    string Email,
    string FirstName,
    string LastName,
    Guid? EntityId,
    string? EntityName,
    Guid[] RoleIds,
    CreateUserAssignmentRequest[] Assignments);

public sealed record CreateUserAssignmentRequest(string ScopeType, Guid ScopeId, string ScopeName);

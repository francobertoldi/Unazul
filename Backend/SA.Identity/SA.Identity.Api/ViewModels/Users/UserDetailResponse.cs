using Shared.Contract.Enums;

namespace SA.Identity.Api.ViewModels.Users;

public sealed record UserDetailResponse(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    Guid? EntityId,
    string? EntityName,
    UserStatus Status,
    int FailedLoginAttempts,
    DateTime? LastLogin,
    string? Avatar,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy,
    IReadOnlyList<UserDetailRoleResponse> Roles,
    IReadOnlyList<UserDetailAssignmentResponse> Assignments,
    IReadOnlyList<UserDetailPermissionResponse>? EffectivePermissions = null);

public sealed record UserDetailRoleResponse(Guid Id, string Name);

public sealed record UserDetailAssignmentResponse(Guid Id, string ScopeType, Guid ScopeId, string ScopeName);

public sealed record UserDetailPermissionResponse(Guid Id, string Module, string Action, string Code, string? Description);

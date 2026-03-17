using Shared.Contract.Enums;

namespace SA.Identity.Application.Dtos.Users;

public sealed record UserDetailDto(
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
    IReadOnlyList<UserDetailRoleDto> Roles,
    IReadOnlyList<UserDetailAssignmentDto> Assignments,
    IReadOnlyList<UserDetailPermissionDto>? EffectivePermissions = null);

public sealed record UserDetailRoleDto(Guid Id, string Name);

public sealed record UserDetailAssignmentDto(Guid Id, string ScopeType, Guid ScopeId, string ScopeName);

public sealed record UserDetailPermissionDto(Guid Id, string Module, string Action, string Code, string? Description);

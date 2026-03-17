using Shared.Contract.Enums;

namespace SA.Identity.Api.ViewModels.Users;

public sealed record UserResponse(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    Guid? EntityId,
    string? EntityName,
    UserStatus Status,
    DateTime? LastLogin,
    string? Avatar,
    IReadOnlyList<string> Roles);

using Shared.Contract.Enums;

namespace SA.Identity.Application.Dtos.Users;

public sealed record UserDto(
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

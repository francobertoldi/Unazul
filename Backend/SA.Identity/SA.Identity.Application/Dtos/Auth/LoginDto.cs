namespace SA.Identity.Application.Dtos.Auth;

public sealed record LoginDto(
    string? AccessToken,
    string? RefreshToken,
    int ExpiresIn,
    Guid UserId,
    string Username,
    string FirstName,
    string LastName,
    string? Avatar,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    bool RequiresOtp = false,
    string? OtpToken = null);

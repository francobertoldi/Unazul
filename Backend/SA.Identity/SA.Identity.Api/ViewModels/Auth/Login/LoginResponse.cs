namespace SA.Identity.Api.ViewModels.Auth.Login;

public sealed record LoginResponse(
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
    bool RequiresOtp,
    string? OtpToken);

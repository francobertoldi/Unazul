namespace SA.Identity.Application.Dtos.Auth;

public sealed record TokenDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);

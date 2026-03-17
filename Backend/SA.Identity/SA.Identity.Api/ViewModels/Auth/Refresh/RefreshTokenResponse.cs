namespace SA.Identity.Api.ViewModels.Auth.Refresh;

public sealed record RefreshTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

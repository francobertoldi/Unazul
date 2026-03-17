namespace SA.Identity.Api.ViewModels.Auth.Otp;

public sealed record VerifyOtpResponse(string AccessToken, string RefreshToken, int ExpiresIn);

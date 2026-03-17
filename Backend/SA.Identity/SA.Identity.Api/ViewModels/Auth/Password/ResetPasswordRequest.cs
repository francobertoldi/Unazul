namespace SA.Identity.Api.ViewModels.Auth.Password;

public sealed record ResetPasswordRequest(string Token, string NewPassword);

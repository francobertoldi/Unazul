namespace SA.Identity.Api.ViewModels.Auth.Otp;

public sealed record VerifyOtpRequest(Guid UserId, string OtpCode);

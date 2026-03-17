using Riok.Mapperly.Abstractions;
using SA.Identity.Api.ViewModels.Auth.Login;
using SA.Identity.Api.ViewModels.Auth.Otp;
using SA.Identity.Api.ViewModels.Auth.Refresh;
using SA.Identity.Application.Dtos.Auth;

namespace SA.Identity.Api.Mappers.Auth;

[Mapper]
public static partial class AuthMapper
{
    public static partial LoginResponse ToLoginResponse(LoginDto dto);
    public static partial VerifyOtpResponse ToVerifyOtpResponse(TokenDto dto);
    public static partial RefreshTokenResponse ToRefreshTokenResponse(TokenDto dto);
}

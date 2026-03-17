using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mediator;
using SA.Identity.Application.Dtos.Auth;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Auth;

public sealed partial class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPermissionRepository permissionRepository,
    IJwtTokenService jwtTokenService,
    IPasswordService passwordService) : ICommandHandler<LoginCommand, LoginDto>
{
    private const string GenericErrorCode = "AUTH_INVALID_CREDENTIALS";
    private const string GenericErrorMessage = "Usuario o contraseña incorrectos.";
    private const string AccountLockedCode = "AUTH_ACCOUNT_LOCKED";
    private const string AccountInactiveCode = "AUTH_ACCOUNT_INACTIVE";
    private const int AccessTokenExpiresInSeconds = 900;

    [GeneratedRegex(@"^[a-zA-Z0-9._\-]{3,30}$")]
    private static partial Regex UsernameRegex();

    public async ValueTask<LoginDto> Handle(LoginCommand command, CancellationToken ct)
    {
        if (!UsernameRegex().IsMatch(command.Username))
        {
            throw new ValidationException(GenericErrorCode, GenericErrorMessage);
        }

        var user = await userRepository.GetByUsernameAsync(command.TenantId, command.Username, ct);

        if (user is null)
        {
            throw new ValidationException(GenericErrorCode, GenericErrorMessage);
        }

        if (user.IsInactive)
        {
            throw new ValidationException(AccountInactiveCode, "La cuenta está inactiva.");
        }

        if (user.IsLocked)
        {
            throw new ValidationException(AccountLockedCode, "La cuenta está bloqueada.");
        }

        if (!passwordService.Verify(command.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            userRepository.Update(user);
            await userRepository.SaveChangesAsync(ct);
            throw new ValidationException(GenericErrorCode, GenericErrorMessage);
        }

        user.RecordSuccessfulLogin();
        userRepository.Update(user);

        // TODO: When OTP is enabled for the tenant, return requires_otp = true
        // with an otp_token (a short-lived JWT with just the user_id).
        // For now, always go through direct login (no OTP).

        var roles = await permissionRepository.GetRoleNamesByUserIdAsync(user.Id, ct);
        var permissions = await permissionRepository.GetPermissionCodesByUserIdAsync(user.Id, ct);

        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id,
            user.TenantId,
            user.Username,
            user.EntityId,
            roles,
            permissions);

        var rawRefreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = ComputeSha256(rawRefreshToken);

        var refreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(7));

        await refreshTokenRepository.AddAsync(refreshToken, ct);
        await refreshTokenRepository.SaveChangesAsync(ct);

        return new LoginDto(
            accessToken,
            rawRefreshToken,
            AccessTokenExpiresInSeconds,
            user.Id,
            user.Username,
            user.FirstName,
            user.LastName,
            user.Avatar,
            roles,
            permissions,
            RequiresOtp: false,
            OtpToken: null);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

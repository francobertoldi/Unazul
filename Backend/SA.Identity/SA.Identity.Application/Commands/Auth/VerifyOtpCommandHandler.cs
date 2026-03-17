using System.Security.Cryptography;
using System.Text;
using Mediator;
using SA.Identity.Application.Dtos.Auth;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Auth;

public sealed class VerifyOtpCommandHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPermissionRepository permissionRepository,
    IJwtTokenService jwtTokenService,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<VerifyOtpCommand, TokenDto>
{
    private const int AccessTokenExpiresInSeconds = 900;

    public async ValueTask<TokenDto> Handle(VerifyOtpCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, ct)
            ?? throw new NotFoundException("USERS_NOT_FOUND", "Usuario no encontrado.");

        if (!user.IsActive)
        {
            throw new ValidationException("AUTH_ACCOUNT_INACTIVE", "La cuenta no está activa.");
        }

        var otpToken = await otpRepository.GetActiveByUserIdAsync(command.UserId, ct)
            ?? throw new NotFoundException("OTP_NOT_FOUND", "No se encontró un código OTP activo.");

        if (otpToken.IsExpired)
        {
            otpToken.MarkAsUsed();
            await otpRepository.UpdateAsync(otpToken, ct);
            throw new ValidationException("OTP_EXPIRED", "El código OTP ha expirado.");
        }

        if (otpToken.IsMaxAttempts)
        {
            otpToken.MarkAsUsed();
            await otpRepository.UpdateAsync(otpToken, ct);
            throw new ValidationException("OTP_MAX_ATTEMPTS", "Se superó el número máximo de intentos.");
        }

        var codeHash = ComputeSha256(command.OtpCode);
        if (otpToken.CodeHash != codeHash)
        {
            otpToken.IncrementAttempt();
            if (otpToken.IsMaxAttempts)
            {
                otpToken.MarkAsUsed();
            }
            await otpRepository.UpdateAsync(otpToken, ct);
            throw new ValidationException("OTP_INVALID", "El código OTP es inválido.");
        }

        // OTP verified successfully
        otpToken.MarkAsUsed();
        await otpRepository.UpdateAsync(otpToken, ct);

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

        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: user.TenantId,
            UserId: user.Id,
            UserName: user.Username,
            Operation: "WRITE",
            Module: "identity",
            Action: "otp_verified",
            Detail: $"OTP verified for user {user.Username}",
            IpAddress: null,
            EntityType: "User",
            EntityId: user.Id,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return new TokenDto(accessToken, rawRefreshToken, AccessTokenExpiresInSeconds);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

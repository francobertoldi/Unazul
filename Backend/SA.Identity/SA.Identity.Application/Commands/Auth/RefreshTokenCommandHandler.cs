using System.Security.Cryptography;
using System.Text;
using Mediator;
using SA.Identity.Application.Dtos.Auth;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;

namespace SA.Identity.Application.Commands.Auth;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IPermissionRepository permissionRepository,
    IJwtTokenService jwtTokenService) : ICommandHandler<RefreshTokenCommand, TokenDto>
{
    private const int AccessTokenExpiresInSeconds = 900;

    public async ValueTask<TokenDto> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var tokenHash = ComputeSha256(command.RefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (storedToken is null || storedToken.IsExpired)
        {
            throw new UnauthorizedAccessException("Token invalido o expirado.");
        }

        // Reuse detection: if token was already revoked, revoke ALL user tokens
        if (storedToken.Revoked)
        {
            await refreshTokenRepository.RevokeAllByUserIdAsync(storedToken.UserId, ct);
            await refreshTokenRepository.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Reuso de token detectado. Todos los tokens han sido revocados.");
        }

        // Revoke the current token
        storedToken.Revoke();

        var user = await userRepository.GetByIdAsync(storedToken.UserId, ct)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("La cuenta no esta activa.");
        }

        // Generate new token pair reflecting current roles/permissions
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
        var newTokenHash = ComputeSha256(rawRefreshToken);

        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            newTokenHash,
            DateTime.UtcNow.AddDays(7));

        await refreshTokenRepository.AddAsync(newRefreshToken, ct);
        await refreshTokenRepository.SaveChangesAsync(ct);

        return new TokenDto(accessToken, rawRefreshToken, AccessTokenExpiresInSeconds);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

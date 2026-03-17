using System.Security.Cryptography;
using System.Text;
using Mediator;
using SA.Identity.DataAccess.Interface.Repositories;

namespace SA.Identity.Application.Commands.Auth;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository) : ICommandHandler<LogoutCommand>
{
    public async ValueTask<Unit> Handle(LogoutCommand command, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            var tokenHash = ComputeSha256(command.RefreshToken);
            var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);
            if (storedToken is not null && !storedToken.Revoked)
            {
                storedToken.Revoke();
                await refreshTokenRepository.SaveChangesAsync(ct);
            }
        }
        else
        {
            // If no specific token provided, revoke all tokens for the user
            await refreshTokenRepository.RevokeAllByUserIdAsync(command.UserId, ct);
            await refreshTokenRepository.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

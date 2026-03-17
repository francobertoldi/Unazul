using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mediator;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Auth;

public sealed partial class ResetPasswordCommandHandler(
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordService passwordService,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<ResetPasswordCommand>
{
    // At least 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char
    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$")]
    private static partial Regex PasswordMaskRegex();

    public async ValueTask<Unit> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var tokenHash = ComputeSha256(command.Token);
        var resetToken = await passwordResetTokenRepository.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new ValidationException("RESET_TOKEN_INVALID", "El token de recuperación es inválido.");

        if (!resetToken.IsValid)
        {
            throw resetToken.Used
                ? new ValidationException("RESET_TOKEN_ALREADY_USED", "El token de recuperación ya fue utilizado.")
                : new ValidationException("RESET_TOKEN_EXPIRED", "El token de recuperación ha expirado.");
        }

        if (!PasswordMaskRegex().IsMatch(command.NewPassword))
        {
            throw new ValidationException("PASSWORD_MASK_INVALID", "La contraseña no cumple con los requisitos de seguridad.");
        }

        var user = await userRepository.GetByIdAsync(resetToken.UserId, ct)
            ?? throw new NotFoundException("USERS_NOT_FOUND", "Usuario no encontrado.");

        var newPasswordHash = passwordService.Hash(command.NewPassword);
        user.ChangePassword(newPasswordHash, user.Id);

        // Reset failed attempts on password change
        if (user.IsLocked)
        {
            user.Update(
                user.Email,
                user.FirstName,
                user.LastName,
                user.EntityId,
                user.EntityName,
                Shared.Contract.Enums.UserStatus.Active,
                user.Avatar,
                user.Id);
        }

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(ct);

        // Revoke ALL user's refresh tokens
        await refreshTokenRepository.RevokeAllByUserIdAsync(user.Id, ct);
        await refreshTokenRepository.SaveChangesAsync(ct);

        // Mark reset token as used
        resetToken.MarkAsUsed();
        await passwordResetTokenRepository.UpdateAsync(resetToken, ct);

        // Publish PasswordChanged domain event
        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: user.TenantId,
            UserId: user.Id,
            UserName: user.Username,
            Operation: "WRITE",
            Module: "identity",
            Action: "password_changed",
            Detail: $"Password reset completed for user {user.Username}",
            IpAddress: null,
            EntityType: "User",
            EntityId: user.Id,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return Unit.Value;
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

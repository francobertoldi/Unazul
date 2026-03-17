using System.Security.Cryptography;
using System.Text;
using Mediator;
using Microsoft.Extensions.Logging;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Events;

namespace SA.Identity.Application.Commands.Auth;

public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IIntegrationEventPublisher eventPublisher,
    ILogger<ForgotPasswordCommandHandler> logger) : ICommandHandler<ForgotPasswordCommand>
{
    public async ValueTask<Unit> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(command.TenantId, command.Email, ct);

        // Always return success to prevent user enumeration
        if (user is null)
        {
            logger.LogWarning("Password recovery requested for unknown email {Email} on tenant {TenantId}",
                command.Email, command.TenantId);
            return Unit.Value;
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Password recovery requested for inactive user {UserId}", user.Id);
            return Unit.Value;
        }

        // Generate a secure random token
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var rawToken = Convert.ToBase64String(randomBytes);
        var tokenHash = ComputeSha256(rawToken);

        var resetToken = PasswordResetToken.Create(user.Id, tokenHash);
        await passwordResetTokenRepository.AddAsync(resetToken, ct);

        // Publish event for notification service to send recovery email with the raw token
        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: command.TenantId,
            UserId: user.Id,
            UserName: user.Username,
            Operation: "WRITE",
            Module: "identity",
            Action: "password_recovery_requested",
            Detail: $"Password recovery requested for {user.Email}",
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

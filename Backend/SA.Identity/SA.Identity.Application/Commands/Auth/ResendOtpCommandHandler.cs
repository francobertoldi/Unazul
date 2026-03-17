using System.Security.Cryptography;
using System.Text;
using Mediator;
using Microsoft.Extensions.Logging;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Identity.Application.Commands.Auth;

public sealed class ResendOtpCommandHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IIntegrationEventPublisher eventPublisher,
    ILogger<ResendOtpCommandHandler> logger) : ICommandHandler<ResendOtpCommand>
{
    public async ValueTask<Unit> Handle(ResendOtpCommand command, CancellationToken ct)
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

        if (!otpToken.CanResend)
        {
            throw new ValidationException("OTP_MAX_RESENDS", "Se superó el número máximo de reenvíos.");
        }

        // Generate new 6-digit OTP code
        var newCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var newCodeHash = ComputeSha256(newCode);

        otpToken.IncrementResend(newCodeHash);
        await otpRepository.UpdateAsync(otpToken, ct);

        logger.LogInformation("OTP resent for user {UserId}. Resend count: {ResendCount}",
            command.UserId, otpToken.ResendCount);

        // Publish event for notification service to send new OTP
        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: user.TenantId,
            UserId: user.Id,
            UserName: user.Username,
            Operation: "WRITE",
            Module: "identity",
            Action: "otp_resent",
            Detail: $"OTP resent for user {user.Username}",
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

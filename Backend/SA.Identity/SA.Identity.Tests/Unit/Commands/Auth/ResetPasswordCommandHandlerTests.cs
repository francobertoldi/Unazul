using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Identity.Application.Commands.Auth;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Events;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Auth;

public sealed class ResetPasswordCommandHandlerTests
{
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository = Substitute.For<IPasswordResetTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly ResetPasswordCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ResetPasswordCommandHandlerTests()
    {
        _sut = new ResetPasswordCommandHandler(
            _passwordResetTokenRepository,
            _userRepository,
            _refreshTokenRepository,
            _passwordService,
            _eventPublisher);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private static User CreateActiveUser()
    {
        return User.Create(TenantId, "testuser", "oldhash", "test@example.com", "Test", "User", null, null, Guid.NewGuid());
    }

    private static PasswordResetToken CreateValidResetToken(Guid userId, string rawToken)
    {
        return PasswordResetToken.Create(userId, ComputeSha256(rawToken));
    }

    [Fact]
    public async Task RF_SEC_04_02_ValidToken_ChangesPasswordAndRevokesRefreshTokens()
    {
        // Arrange
        var user = CreateActiveUser();
        var rawToken = "valid-reset-token";
        var resetToken = CreateValidResetToken(user.Id, rawToken);
        var newPassword = "NewPass1!";

        _passwordResetTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(resetToken);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.Hash(newPassword).Returns("newhash");

        // Act
        await _sut.Handle(new ResetPasswordCommand(rawToken, newPassword), CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be("newhash");
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).RevokeAllByUserIdAsync(user.Id, Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        resetToken.Used.Should().BeTrue();
        await _eventPublisher.Received(1).PublishAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_04_04_ExpiredToken_ThrowsResetTokenExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rawToken = "expired-reset-token";
        var resetToken = PasswordResetToken.Create(userId, ComputeSha256(rawToken));
        // Force expired
        typeof(PasswordResetToken).GetProperty("ExpiresAt")!.SetValue(resetToken, DateTime.UtcNow.AddMinutes(-1));

        _passwordResetTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(resetToken);

        // Act
        Func<Task> act = async () => await _sut.Handle(new ResetPasswordCommand(rawToken, "NewPass1!"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("RESET_TOKEN_EXPIRED");
    }

    [Fact]
    public async Task RF_SEC_04_05_AlreadyUsedToken_ThrowsResetTokenAlreadyUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rawToken = "used-reset-token";
        var resetToken = PasswordResetToken.Create(userId, ComputeSha256(rawToken));
        resetToken.MarkAsUsed(); // Already used

        _passwordResetTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(resetToken);

        // Act
        Func<Task> act = async () => await _sut.Handle(new ResetPasswordCommand(rawToken, "NewPass1!"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("RESET_TOKEN_ALREADY_USED");
    }

    [Fact]
    public async Task RF_SEC_04_06_PasswordDoesNotMeetMask_ThrowsPasswordMaskInvalid()
    {
        // Arrange
        var user = CreateActiveUser();
        var rawToken = "valid-reset-token";
        var resetToken = CreateValidResetToken(user.Id, rawToken);

        _passwordResetTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(resetToken);

        // Act - password too simple (no uppercase, no special char)
        Func<Task> act = async () => await _sut.Handle(new ResetPasswordCommand(rawToken, "weakpass"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("PASSWORD_MASK_INVALID");
    }

    [Fact]
    public async Task RF_SEC_04_07_NonExistentToken_ThrowsResetTokenInvalid()
    {
        // Arrange
        _passwordResetTokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(new ResetPasswordCommand("nonexistent-token", "NewPass1!"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("RESET_TOKEN_INVALID");
    }
}

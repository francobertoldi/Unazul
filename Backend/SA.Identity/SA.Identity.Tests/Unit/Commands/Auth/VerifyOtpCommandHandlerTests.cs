using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Identity.Application.Commands.Auth;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Auth;

public sealed class VerifyOtpCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IOtpRepository _otpRepository = Substitute.For<IOtpRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly VerifyOtpCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public VerifyOtpCommandHandlerTests()
    {
        _sut = new VerifyOtpCommandHandler(
            _userRepository,
            _otpRepository,
            _refreshTokenRepository,
            _permissionRepository,
            _jwtTokenService,
            _eventPublisher);
    }

    private static User CreateActiveUser()
    {
        return User.Create(TenantId, "testuser", "hash", "test@example.com", "Test", "User", null, null, Guid.NewGuid());
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    [Fact]
    public async Task RF_SEC_02_01_ValidOtpWithinTTL_ReturnsJwtAndRefreshToken()
    {
        // Arrange
        var user = CreateActiveUser();
        var otpCode = "123456";
        var otp = OtpToken.Create(user.Id, ComputeSha256(otpCode));

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _otpRepository.GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(otp);
        _permissionRepository.GetRoleNamesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "Admin" });
        _permissionRepository.GetPermissionCodesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "users.read" });
        _jwtTokenService.GenerateAccessToken(
                user.Id, user.TenantId, user.Username, user.EntityId,
                Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns("jwt-token");
        _jwtTokenService.GenerateRefreshToken().Returns("raw-refresh");

        // Act
        var result = await _sut.Handle(new VerifyOtpCommand(TenantId, user.Id, otpCode), CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().Be("raw-refresh");
        result.ExpiresIn.Should().Be(900);
        await _eventPublisher.Received(1).PublishAsync(Arg.Any<Shared.Contract.Events.DomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_02_02_ResendOtp_GeneratesNewCode()
    {
        // This test covers the ResendOtpCommandHandler; see ResendOtpCommandHandlerTests for full coverage.
        // Here we verify that after a resend, verifying with the NEW code works.
        var user = CreateActiveUser();
        var originalCode = "111111";
        var otp = OtpToken.Create(user.Id, ComputeSha256(originalCode));

        // Simulate resend with new code
        var newCode = "222222";
        otp.IncrementResend(ComputeSha256(newCode));

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _otpRepository.GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(otp);
        _permissionRepository.GetRoleNamesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string>());
        _permissionRepository.GetPermissionCodesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string>());
        _jwtTokenService.GenerateAccessToken(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(),
                Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns("jwt");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh");

        // Act - verifying with NEW code should succeed
        var result = await _sut.Handle(new VerifyOtpCommand(TenantId, user.Id, newCode), CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("jwt");
    }

    [Fact]
    public async Task RF_SEC_02_03_IncorrectOtp_ThrowsOtpInvalid()
    {
        // Arrange
        var user = CreateActiveUser();
        var otp = OtpToken.Create(user.Id, ComputeSha256("123456"));

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _otpRepository.GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(otp);

        // Act
        Func<Task> act = async () => await _sut.Handle(new VerifyOtpCommand(TenantId, user.Id, "999999"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OTP_INVALID");
        await _otpRepository.Received(1).UpdateAsync(otp, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_02_04_ExpiredOtp_ThrowsOtpExpired()
    {
        // Arrange
        var user = CreateActiveUser();
        // Create OTP that is already expired by using reflection or a workaround.
        // OtpToken.Create sets ExpiresAt = UtcNow + 5min, so we create and manually expire it.
        var otp = OtpToken.Create(user.Id, ComputeSha256("123456"));
        // Force expiration by accessing private setter via reflection
        typeof(OtpToken).GetProperty("ExpiresAt")!.SetValue(otp, DateTime.UtcNow.AddMinutes(-1));

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _otpRepository.GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(otp);

        // Act
        Func<Task> act = async () => await _sut.Handle(new VerifyOtpCommand(TenantId, user.Id, "123456"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OTP_EXPIRED");
    }

    [Fact]
    public async Task RF_SEC_02_05_ThirdFailedAttempt_InvalidatesOtpToken()
    {
        // Arrange
        var user = CreateActiveUser();
        var otp = OtpToken.Create(user.Id, ComputeSha256("123456"));
        // Simulate 2 prior failed attempts
        otp.IncrementAttempt();
        otp.IncrementAttempt();

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _otpRepository.GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(otp);

        // Act - 3rd wrong attempt
        Func<Task> act = async () => await _sut.Handle(new VerifyOtpCommand(TenantId, user.Id, "wrong1"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OTP_INVALID");
        otp.IsMaxAttempts.Should().BeTrue();
        otp.Used.Should().BeTrue(); // marked as used after max attempts
    }

    [Fact]
    public async Task RF_SEC_02_06_NoActiveOtp_ThrowsOtpNotFound()
    {
        // Arrange
        var user = CreateActiveUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _otpRepository.GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(new VerifyOtpCommand(TenantId, user.Id, "123456"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OTP_NOT_FOUND");
    }
}

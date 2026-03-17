using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Identity.Application.Commands.Auth;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Auth;

public sealed class LogoutCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly LogoutCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public LogoutCommandHandlerTests()
    {
        _sut = new LogoutCommandHandler(_refreshTokenRepository);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    [Fact]
    public async Task RF_SEC_05_01_LogoutWithValidToken_RevokesRefreshTokenAndReturnsSuccess()
    {
        // Arrange
        var rawToken = "valid-refresh-token";
        var storedToken = RefreshToken.Create(UserId, ComputeSha256(rawToken), DateTime.UtcNow.AddDays(7));

        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(storedToken);

        // Act
        await _sut.Handle(new LogoutCommand(UserId, rawToken), CancellationToken.None);

        // Assert
        storedToken.Revoked.Should().BeTrue();
        await _refreshTokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_05_02_LogoutWithAlreadyRevokedToken_IsIdempotent()
    {
        // Arrange
        var rawToken = "already-revoked-token";
        var storedToken = RefreshToken.Create(UserId, ComputeSha256(rawToken), DateTime.UtcNow.AddDays(7));
        storedToken.Revoke(); // Already revoked

        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(storedToken);

        // Act - should NOT throw
        Func<Task> act = async () => await _sut.Handle(new LogoutCommand(UserId, rawToken), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        // SaveChanges should NOT be called since token was already revoked
        await _refreshTokenRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_05_03_LogoutWithOtherUsersToken_HandlerDoesNotCheckOwnership()
    {
        // Note: The current LogoutCommandHandler does not verify token ownership.
        // This test documents the current behavior. The endpoint layer should
        // enforce that the token belongs to the authenticated user.
        // The handler simply revokes whatever token it finds by hash.
        var rawToken = "other-users-token";
        var otherUserId = Guid.NewGuid();
        var storedToken = RefreshToken.Create(otherUserId, ComputeSha256(rawToken), DateTime.UtcNow.AddDays(7));

        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(storedToken);

        // Act - handler revokes it regardless (ownership check is at endpoint level)
        await _sut.Handle(new LogoutCommand(UserId, rawToken), CancellationToken.None);

        // Assert
        storedToken.Revoked.Should().BeTrue();
    }

    [Fact]
    public async Task RF_SEC_05_04_LogoutWithoutRefreshToken_RevokesAllUserTokens()
    {
        // Note: This test covers the case where no specific token is provided.
        // The Authorization header check (RF-SEC-05-04) is at endpoint level.
        // At handler level, a null/empty refresh token causes revocation of all user tokens.

        // Act
        await _sut.Handle(new LogoutCommand(UserId, null), CancellationToken.None);

        // Assert
        await _refreshTokenRepository.Received(1)
            .RevokeAllByUserIdAsync(UserId, Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_05_05_LogoutWithEmptyRefreshToken_RevokesAllUserTokens()
    {
        // Arrange - empty string refresh token

        // Act
        await _sut.Handle(new LogoutCommand(UserId, ""), CancellationToken.None);

        // Assert
        await _refreshTokenRepository.Received(1)
            .RevokeAllByUserIdAsync(UserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_05_06_LogoutWithNonExistentToken_IsIdempotent()
    {
        // Arrange
        var rawToken = "nonexistent-token";
        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act - should NOT throw
        Func<Task> act = async () => await _sut.Handle(new LogoutCommand(UserId, rawToken), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await _refreshTokenRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

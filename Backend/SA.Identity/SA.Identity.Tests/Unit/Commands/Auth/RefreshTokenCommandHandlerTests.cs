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

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly RefreshTokenCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public RefreshTokenCommandHandlerTests()
    {
        _sut = new RefreshTokenCommandHandler(
            _refreshTokenRepository,
            _userRepository,
            _permissionRepository,
            _jwtTokenService);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private static User CreateActiveUser()
    {
        return User.Create(TenantId, "testuser", "hash", "test@example.com", "Test", "User", null, null, Guid.NewGuid());
    }

    private static RefreshToken CreateValidRefreshToken(Guid userId, string rawToken)
    {
        return RefreshToken.Create(userId, ComputeSha256(rawToken), DateTime.UtcNow.AddDays(7));
    }

    [Fact]
    public async Task RF_SEC_03_01_ValidRefreshToken_ReturnsNewJwtAndRefreshToken()
    {
        // Arrange
        var user = CreateActiveUser();
        var rawToken = "valid-refresh-token";
        var storedToken = CreateValidRefreshToken(user.Id, rawToken);

        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(storedToken);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _permissionRepository.GetRoleNamesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "Admin" });
        _permissionRepository.GetPermissionCodesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "users.read" });
        _jwtTokenService.GenerateAccessToken(
                user.Id, user.TenantId, user.Username, user.EntityId,
                Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns("new-jwt-token");
        _jwtTokenService.GenerateRefreshToken().Returns("new-raw-refresh");

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("new-jwt-token");
        result.RefreshToken.Should().Be("new-raw-refresh");
        result.ExpiresIn.Should().Be(900);
        storedToken.Revoked.Should().BeTrue(); // old token revoked
    }

    [Fact]
    public async Task RF_SEC_03_02_UpdatedPermissions_ReflectedInNewJwt()
    {
        // Arrange
        var user = CreateActiveUser();
        var rawToken = "valid-refresh-token";
        var storedToken = CreateValidRefreshToken(user.Id, rawToken);

        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(storedToken);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        // User now has new roles/permissions (updated between refreshes)
        var newRoles = new List<string> { "Admin", "Manager" };
        var newPerms = new List<string> { "users.read", "users.write", "reports.view" };
        _permissionRepository.GetRoleNamesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(newRoles);
        _permissionRepository.GetPermissionCodesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(newPerms);
        _jwtTokenService.GenerateAccessToken(
                user.Id, user.TenantId, user.Username, user.EntityId,
                Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns("jwt-with-new-perms");
        _jwtTokenService.GenerateRefreshToken().Returns("new-refresh");

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("jwt-with-new-perms");
        _jwtTokenService.Received(1).GenerateAccessToken(
            user.Id, user.TenantId, user.Username, user.EntityId,
            Arg.Is<IReadOnlyList<string>>(r => r.Count == 2),
            Arg.Is<IReadOnlyList<string>>(p => p.Count == 3));
    }

    [Fact]
    public async Task RF_SEC_03_03_RevokedToken_RevokesAllUserTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rawToken = "already-revoked-token";
        var storedToken = RefreshToken.Create(userId, ComputeSha256(rawToken), DateTime.UtcNow.AddDays(7));
        storedToken.Revoke(); // Already revoked (reuse detection)

        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(storedToken);

        // Act
        Func<Task> act = async () => await _sut.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await _refreshTokenRepository.Received(1).RevokeAllByUserIdAsync(userId, Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_03_04_ExpiredToken_ThrowsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rawToken = "expired-token";
        var storedToken = RefreshToken.Create(userId, ComputeSha256(rawToken), DateTime.UtcNow.AddDays(7));
        // Force expired via reflection
        typeof(RefreshToken).GetProperty("ExpiresAt")!.SetValue(storedToken, DateTime.UtcNow.AddMinutes(-1));

        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(storedToken);

        // Act
        Func<Task> act = async () => await _sut.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RF_SEC_03_05_NonExistentToken_ThrowsUnauthorized()
    {
        // Arrange
        _refreshTokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(new RefreshTokenCommand("nonexistent-token"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RF_SEC_03_06_UserLockedBetweenRefreshes_ThrowsUnauthorized()
    {
        // Arrange
        var user = CreateActiveUser();
        // Lock the user after creating the token
        for (int i = 0; i < 5; i++) user.RecordFailedLogin();

        var rawToken = "valid-token";
        var storedToken = CreateValidRefreshToken(user.Id, rawToken);

        _refreshTokenRepository.GetByTokenHashAsync(ComputeSha256(rawToken), Arg.Any<CancellationToken>())
            .Returns(storedToken);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        Func<Task> act = async () => await _sut.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}

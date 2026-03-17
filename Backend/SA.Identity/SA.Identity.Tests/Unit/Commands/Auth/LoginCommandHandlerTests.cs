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

public sealed class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly LoginCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(
            _userRepository,
            _refreshTokenRepository,
            _permissionRepository,
            _jwtTokenService,
            _passwordService);
    }

    private static User CreateActiveUser(string username = "testuser", string passwordHash = "hashed")
    {
        return User.Create(TenantId, username, passwordHash, "test@example.com", "Test", "User", null, null, Guid.NewGuid());
    }

    private static User CreateLockedUser()
    {
        var user = CreateActiveUser();
        for (int i = 0; i < 5; i++) user.RecordFailedLogin();
        return user;
    }

    private static User CreateInactiveUser()
    {
        var user = CreateActiveUser();
        user.Update(user.Email, user.FirstName, user.LastName, user.EntityId, user.EntityName,
            UserStatus.Inactive, user.Avatar, Guid.NewGuid());
        return user;
    }

    [Fact]
    public async Task RF_SEC_01_01_LoginWithValidCredentials_ReturnsJwtAndRefreshToken()
    {
        // Arrange
        var user = CreateActiveUser();
        _userRepository.GetByUsernameAsync(TenantId, "testuser", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordService.Verify("Password1!", user.PasswordHash).Returns(true);
        _permissionRepository.GetRoleNamesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "Admin" });
        _permissionRepository.GetPermissionCodesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "users.read" });
        _jwtTokenService.GenerateAccessToken(
                user.Id, user.TenantId, user.Username, user.EntityId,
                Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("raw-refresh-token");

        // Act
        var result = await _sut.Handle(new LoginCommand(TenantId, "testuser", "Password1!"), CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("jwt-access-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresIn.Should().Be(900);
        result.UserId.Should().Be(user.Id);
        result.Username.Should().Be("testuser");
        result.Roles.Should().Contain("Admin");
        result.Permissions.Should().Contain("users.read");
        result.RequiresOtp.Should().BeFalse();
    }

    [Fact]
    public async Task RF_SEC_01_02_LoginSuccess_ResetsFailedAttemptsAndUpdatesLastLogin()
    {
        // Arrange
        var user = CreateActiveUser();
        user.RecordFailedLogin();
        user.RecordFailedLogin();
        _userRepository.GetByUsernameAsync(TenantId, "testuser", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordService.Verify("Password1!", user.PasswordHash).Returns(true);
        _permissionRepository.GetRoleNamesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string>());
        _permissionRepository.GetPermissionCodesByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<string>());
        _jwtTokenService.GenerateAccessToken(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(),
                Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>())
            .Returns("token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh");

        // Act
        await _sut.Handle(new LoginCommand(TenantId, "testuser", "Password1!"), CancellationToken.None);

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LastLogin.Should().NotBeNull();
        _userRepository.Received(1).Update(user);
    }

    [Fact]
    public async Task RF_SEC_01_03_InvalidPassword_ThrowsWithGenericMessage()
    {
        // Arrange
        var user = CreateActiveUser();
        _userRepository.GetByUsernameAsync(TenantId, "testuser", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordService.Verify("wrongpassword", user.PasswordHash).Returns(false);

        // Act
        Func<Task> act = async () => await _sut.Handle(new LoginCommand(TenantId, "testuser", "wrongpassword"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Usuario o contrasena incorrectos");
    }

    [Fact]
    public async Task RF_SEC_01_04_NonExistentUser_ThrowsWithGenericMessage()
    {
        // Arrange
        _userRepository.GetByUsernameAsync(TenantId, "noexiste", Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(new LoginCommand(TenantId, "noexiste", "Password1!"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Usuario o contrasena incorrectos");
    }

    [Fact]
    public async Task RF_SEC_01_05_LockedAccount_ThrowsAccountLocked()
    {
        // Arrange
        var user = CreateLockedUser();
        _userRepository.GetByUsernameAsync(TenantId, "testuser", Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        Func<Task> act = async () => await _sut.Handle(new LoginCommand(TenantId, "testuser", "Password1!"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUTH_ACCOUNT_LOCKED");
    }

    [Fact]
    public async Task RF_SEC_01_06_InactiveAccount_ThrowsAccountInactive()
    {
        // Arrange
        var user = CreateInactiveUser();
        _userRepository.GetByUsernameAsync(TenantId, "testuser", Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        Func<Task> act = async () => await _sut.Handle(new LoginCommand(TenantId, "testuser", "Password1!"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUTH_ACCOUNT_INACTIVE");
    }

    [Fact]
    public async Task RF_SEC_01_07_FifthFailedAttempt_LocksAccountPermanently()
    {
        // Arrange
        var user = CreateActiveUser();
        // Simulate 4 prior failed attempts
        for (int i = 0; i < 4; i++) user.RecordFailedLogin();

        _userRepository.GetByUsernameAsync(TenantId, "testuser", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordService.Verify("wrong", user.PasswordHash).Returns(false);

        // Act
        Func<Task> act = async () => await _sut.Handle(new LoginCommand(TenantId, "testuser", "wrong"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        user.IsLocked.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(5);
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_01_08_InvalidUsernameFormat_ThrowsWithGenericMessage()
    {
        // Arrange - username with special chars that don't match the regex
        Func<Task> act = async () => await _sut.Handle(new LoginCommand(TenantId, "user@invalid!", "Password1!"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Usuario o contrasena incorrectos");

        // Should never hit the repository
        await _userRepository.DidNotReceive().GetByUsernameAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}

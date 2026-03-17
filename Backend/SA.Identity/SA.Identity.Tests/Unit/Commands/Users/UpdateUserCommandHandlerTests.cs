using FluentAssertions;
using NSubstitute;
using SA.Identity.Application.Commands.Users;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Users;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly UpdateUserCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid UpdatedBy = Guid.NewGuid();
    private static readonly Guid RoleId = Guid.NewGuid();

    public UpdateUserCommandHandlerTests()
    {
        _sut = new UpdateUserCommandHandler(_userRepository, _eventPublisher);
    }

    private UpdateUserCommand BuildCommand(
        Guid? userId = null,
        string email = "updated@example.com",
        UserStatus status = UserStatus.Active,
        Guid[]? roleIds = null,
        UpdateUserAssignmentInput[]? assignments = null)
    {
        return new UpdateUserCommand(
            userId ?? UserId,
            TenantId,
            email,
            "UpdatedFirst",
            "UpdatedLast",
            null,
            null,
            status,
            null,
            roleIds ?? [RoleId],
            assignments ?? [],
            UpdatedBy);
    }

    private static User CreateExistingUser(bool withSuperAdminRole = false)
    {
        var user = User.Create(TenantId, "existing.user", "hashed", "old@example.com", "Old", "Name", null, null, Guid.NewGuid());
        if (withSuperAdminRole)
        {
            var role = Role.Create(TenantId, "Super Admin", "System admin role", Guid.NewGuid());
            // Role.IsSystem is false by default from Create; we need reflection to set it for testing
            SetPrivateProperty(role, nameof(Role.IsSystem), true);
            var userRole = UserRole.Create(user.Id, role.Id);
            SetPrivateProperty(userRole, nameof(UserRole.Role), role);
            ((ICollection<UserRole>)user.UserRoles).Add(userRole);
        }
        return user;
    }

    private static void SetPrivateProperty<T>(T obj, string propertyName, object value) where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        prop!.SetValue(obj, value);
    }

    /// <summary>
    /// TP-SEC-07-02: PUT /users/:id actualiza datos, roles y asignaciones
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_UpdatesUserDataRolesAndAssignments()
    {
        // Arrange
        var existingUser = CreateExistingUser();
        var command = BuildCommand(
            userId: existingUser.Id,
            assignments: [new UpdateUserAssignmentInput("branch", Guid.NewGuid(), "Branch 1")]);

        _userRepository.GetByIdWithRolesAsync(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByIdWithRolesAsync(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _userRepository.Received(1).Update(existingUser);
        await _userRepository.Received(1).SetUserRolesAsync(
            existingUser.Id, command.RoleIds, Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SetUserAssignmentsAsync(
            existingUser.Id, Arg.Any<IEnumerable<UserAssignment>>(), Arg.Any<CancellationToken>());
        await _userRepository.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-07-05b: Email duplicado en update retorna 409
    /// </summary>
    [Fact]
    public async Task Handle_DuplicateEmailOtherUser_ThrowsConflict()
    {
        // Arrange
        var existingUser = CreateExistingUser();
        var otherUser = User.Create(TenantId, "other", "hashed", "updated@example.com", "Other", "User", null, null, Guid.NewGuid());
        var command = BuildCommand(userId: existingUser.Id);

        _userRepository.GetByIdWithRolesAsync(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns(otherUser);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email ya esta en uso*");
    }

    /// <summary>
    /// TP-SEC-07-02b: Usuario inexistente retorna 404
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        // Arrange
        var command = BuildCommand();
        _userRepository.GetByIdWithRolesAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no encontrado*");
    }

    /// <summary>
    /// TP-SEC-07-02c: Same email on same user is allowed
    /// </summary>
    [Fact]
    public async Task Handle_SameEmailSameUser_DoesNotThrow()
    {
        // Arrange
        var existingUser = CreateExistingUser();
        var command = BuildCommand(userId: existingUser.Id, email: "updated@example.com");

        _userRepository.GetByIdWithRolesAsync(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        // Same user returned by email lookup
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// TP-SEC-07-02d: Last Super Admin cannot be deactivated
    /// </summary>
    [Fact]
    public async Task Handle_LastSuperAdminDeactivation_Throws()
    {
        // Arrange
        var existingUser = CreateExistingUser(withSuperAdminRole: true);
        var command = BuildCommand(userId: existingUser.Id, status: UserStatus.Inactive);

        _userRepository.GetByIdWithRolesAsync(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.CountActiveSuperAdminsByTenantAsync(TenantId, existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*USERS_LAST_SUPER_ADMIN*");
    }

    /// <summary>
    /// TP-SEC-07-02e: Multiple Super Admins allows deactivation
    /// </summary>
    [Fact]
    public async Task Handle_MultipleSuperAdmins_AllowsDeactivation()
    {
        // Arrange
        var existingUser = CreateExistingUser(withSuperAdminRole: true);
        var command = BuildCommand(userId: existingUser.Id, status: UserStatus.Inactive);

        _userRepository.GetByIdWithRolesAsync(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.CountActiveSuperAdminsByTenantAsync(TenantId, existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}

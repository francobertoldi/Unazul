using FluentAssertions;
using NSubstitute;
using SA.Identity.Application.Dtos.Users;
using SA.Identity.Application.Queries.Users;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Xunit;

namespace SA.Identity.Tests.Unit.Queries.Users;

public sealed class GetUserDetailQueryHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly GetUserDetailQueryHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public GetUserDetailQueryHandlerTests()
    {
        _sut = new GetUserDetailQueryHandler(_userRepository, _permissionRepository);
    }

    private static void SetPrivateProperty<T>(T obj, string propertyName, object value) where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        prop!.SetValue(obj, value);
    }

    private static User CreateUserWithRolesAndPermissions(
        Guid userId,
        int roleCount = 2,
        int permissionsPerRole = 3)
    {
        var user = User.Create(TenantId, "testuser", "hash", "test@example.com", "Test", "User", null, null, Guid.NewGuid());
        SetPrivateProperty(user, nameof(User.Id), userId);

        var roles = new List<UserRole>();
        for (int r = 0; r < roleCount; r++)
        {
            var role = Role.Create(TenantId, $"Role{r}", $"Desc{r}", Guid.NewGuid());
            var userRole = UserRole.Create(userId, role.Id);
            SetPrivateProperty(userRole, nameof(UserRole.Role), role);
            roles.Add(userRole);
        }

        foreach (var ur in roles)
            ((ICollection<UserRole>)user.UserRoles).Add(ur);

        var assignment = UserAssignment.Create(userId, "branch", Guid.NewGuid(), "Branch 1");
        ((ICollection<UserAssignment>)user.Assignments).Add(assignment);

        return user;
    }

    private static List<Permission> CreatePermissions(int count, bool withDuplicates = false)
    {
        var permissions = new List<Permission>();
        for (int i = 0; i < count; i++)
        {
            permissions.Add(Permission.Create("module", $"action_{i}", $"p_code_{i}", $"Description {i}"));
        }
        if (withDuplicates && count > 0)
        {
            // The repository should return DISTINCT already, but we test the mapping
            permissions.Add(permissions[0]);
        }
        return permissions;
    }

    /// <summary>
    /// TP-SEC-08-01: GET /users/:id retorna datos completos + roles + permisos efectivos
    /// </summary>
    [Fact]
    public async Task Handle_ExistingUser_ReturnsFullDetailWithRolesAndPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithRolesAndPermissions(userId, roleCount: 2);
        var permissions = CreatePermissions(5);

        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _permissionRepository.GetEffectivePermissionsByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(permissions);

        var query = new GetUserDetailQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Username.Should().Be("testuser");
        result.Roles.Should().HaveCount(2);
        result.Assignments.Should().HaveCount(1);
        result.EffectivePermissions.Should().NotBeNull();
        result.EffectivePermissions!.Should().HaveCount(5);
    }

    /// <summary>
    /// TP-SEC-08-02: Permisos efectivos son DISTINCT union de todos los roles
    /// </summary>
    [Fact]
    public async Task Handle_ExistingUser_EffectivePermissionsAreDistinct()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithRolesAndPermissions(userId, roleCount: 2);
        var permissions = CreatePermissions(3); // Repository returns distinct

        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _permissionRepository.GetEffectivePermissionsByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(permissions);

        var query = new GetUserDetailQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.EffectivePermissions.Should().NotBeNull();
        result.EffectivePermissions!.Should().HaveCount(3);
        result.EffectivePermissions!.Select(p => p.Code).Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// TP-SEC-08-03: Usuario inexistente retorna 404
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var query = new GetUserDetailQuery(userId);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no encontrado*");
    }

    /// <summary>
    /// TP-SEC-08-01b: Permissions include Module, Action, Code, Description
    /// </summary>
    [Fact]
    public async Task Handle_ExistingUser_PermissionDtoFieldsArePopulated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithRolesAndPermissions(userId, roleCount: 1);
        var permission = Permission.Create("identity", "read", "p_users_list", "Can list users");
        var permissions = new List<Permission> { permission };

        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _permissionRepository.GetEffectivePermissionsByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(permissions);

        var query = new GetUserDetailQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var perm = result.EffectivePermissions!.Single();
        perm.Module.Should().Be("identity");
        perm.Action.Should().Be("read");
        perm.Code.Should().Be("p_users_list");
        perm.Description.Should().Be("Can list users");
    }

    /// <summary>
    /// TP-SEC-08-01c: User with no roles returns empty permissions
    /// </summary>
    [Fact]
    public async Task Handle_UserWithNoRoles_ReturnsEmptyPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithRolesAndPermissions(userId, roleCount: 0);

        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _permissionRepository.GetEffectivePermissionsByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Permission>());

        var query = new GetUserDetailQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().BeEmpty();
        result.EffectivePermissions.Should().NotBeNull();
        result.EffectivePermissions!.Should().BeEmpty();
    }
}

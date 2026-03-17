using FluentAssertions;
using NSubstitute;
using SA.Identity.Application.Queries.Roles;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Xunit;

namespace SA.Identity.Tests.Unit.Queries.Roles;

public sealed class ListRolesQueryHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly ListRolesQueryHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ListRolesQueryHandlerTests()
    {
        _sut = new ListRolesQueryHandler(_roleRepository);
    }

    private static void SetPrivateProperty<T>(T obj, string propertyName, object value) where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        prop!.SetValue(obj, value);
    }

    private static Role CreateRole(string name, bool isSystem = false, int permissionCount = 0, int userCount = 0)
    {
        var role = Role.Create(TenantId, name, $"Description of {name}", Guid.NewGuid());
        if (isSystem)
        {
            SetPrivateProperty(role, nameof(Role.IsSystem), true);
        }

        // Add fake RolePermissions
        for (int i = 0; i < permissionCount; i++)
        {
            var perm = Permission.Create("module", $"action_{i}", $"code_{i}", null);
            var rp = RolePermission.Create(role.Id, perm.Id);
            SetPrivateProperty(rp, nameof(RolePermission.Permission), perm);
            ((ICollection<RolePermission>)role.RolePermissions).Add(rp);
        }

        // Add fake UserRoles
        for (int i = 0; i < userCount; i++)
        {
            var ur = UserRole.Create(Guid.NewGuid(), role.Id);
            ((ICollection<UserRole>)role.UserRoles).Add(ur);
        }

        return role;
    }

    /// <summary>
    /// TP-SEC-09-01: GET /roles retorna lista paginada con campos esperados
    /// </summary>
    [Fact]
    public async Task Handle_ValidQuery_ReturnsPaginatedList()
    {
        // Arrange
        var roles = new List<Role>
        {
            CreateRole("Admin", isSystem: true, permissionCount: 5, userCount: 2),
            CreateRole("Viewer", permissionCount: 3, userCount: 1)
        };

        _roleRepository.ListAsync(TenantId, 0, 10, null, null, null, Arg.Any<CancellationToken>())
            .Returns((roles.AsReadOnly() as IReadOnlyList<Role>, 2));

        var query = new ListRolesQuery(TenantId, 1, 10, null, null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    /// <summary>
    /// TP-SEC-09-02: Cada rol incluye permission_count y user_count
    /// </summary>
    [Fact]
    public async Task Handle_ValidQuery_EachRoleIncludesPermissionCountAndUserCount()
    {
        // Arrange
        var role = CreateRole("Admin", permissionCount: 7, userCount: 3);
        _roleRepository.ListAsync(TenantId, 0, 10, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Role> { role }.AsReadOnly() as IReadOnlyList<Role>, 1));

        var query = new ListRolesQuery(TenantId, 1, 10, null, null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Items.Single();
        dto.PermissionCount.Should().Be(7);
        dto.UserCount.Should().Be(3);
    }

    /// <summary>
    /// TP-SEC-09-03: Busqueda por nombre/descripcion filtra correctamente
    /// </summary>
    [Fact]
    public async Task Handle_SearchQuery_PassesSearchToRepository()
    {
        // Arrange
        var roles = new List<Role> { CreateRole("Admin") };
        _roleRepository.ListAsync(TenantId, 0, 10, "Admin", null, null, Arg.Any<CancellationToken>())
            .Returns((roles.AsReadOnly() as IReadOnlyList<Role>, 1));

        var query = new ListRolesQuery(TenantId, 1, 10, "Admin", null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        await _roleRepository.Received(1).ListAsync(
            TenantId, 0, 10, "Admin", null, null, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-09-04: Roles seed aparecen con is_system = true
    /// </summary>
    [Fact]
    public async Task Handle_SystemRoles_ReturnWithIsSystemTrue()
    {
        // Arrange
        var systemRole = CreateRole("Super Admin", isSystem: true);
        var customRole = CreateRole("Custom Role");
        var roles = new List<Role> { systemRole, customRole };

        _roleRepository.ListAsync(TenantId, 0, 10, null, null, null, Arg.Any<CancellationToken>())
            .Returns((roles.AsReadOnly() as IReadOnlyList<Role>, 2));

        var query = new ListRolesQuery(TenantId, 1, 10, null, null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().Contain(r => r.Name == "Super Admin" && r.IsSystem);
        result.Items.Should().Contain(r => r.Name == "Custom Role" && !r.IsSystem);
    }

    /// <summary>
    /// TP-SEC-09-07: page_size > 100 se clampea a 100
    /// </summary>
    [Fact]
    public async Task Handle_PageSizeOver100_ClampsTo100()
    {
        // Arrange
        _roleRepository.ListAsync(TenantId, 0, 100, null, null, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Role>() as IReadOnlyList<Role>, 0));

        var query = new ListRolesQuery(TenantId, 1, 200, null, null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(100);
        await _roleRepository.Received(1).ListAsync(
            TenantId, 0, 100, null, null, null, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-09-07b: page_size = 0 se clampea a 1
    /// </summary>
    [Fact]
    public async Task Handle_PageSizeZero_ClampsTo1()
    {
        // Arrange
        _roleRepository.ListAsync(TenantId, 0, 1, null, null, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Role>() as IReadOnlyList<Role>, 0));

        var query = new ListRolesQuery(TenantId, 1, 0, null, null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(1);
    }

    /// <summary>
    /// TP-SEC-09-01b: Export mode uses MaxExportSize
    /// </summary>
    [Fact]
    public async Task Handle_ExportMode_UsesMaxExportSize()
    {
        // Arrange
        _roleRepository.ListAsync(TenantId, 0, 10_000, null, null, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Role>() as IReadOnlyList<Role>, 0));

        var query = new ListRolesQuery(TenantId, 1, 10, null, null, null, true);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10_000);
        await _roleRepository.Received(1).ListAsync(
            TenantId, 0, 10_000, null, null, null, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-09-01c: Pagination correctly calculates skip
    /// </summary>
    [Fact]
    public async Task Handle_Page2_CalculatesCorrectSkip()
    {
        // Arrange
        _roleRepository.ListAsync(TenantId, 10, 10, null, null, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Role>() as IReadOnlyList<Role>, 25));

        var query = new ListRolesQuery(TenantId, 2, 10, null, null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Page.Should().Be(2);
        await _roleRepository.Received(1).ListAsync(
            TenantId, 10, 10, null, null, null, Arg.Any<CancellationToken>());
    }
}

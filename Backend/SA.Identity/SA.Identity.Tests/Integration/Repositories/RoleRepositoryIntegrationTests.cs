using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SA.Identity.DataAccess.EntityFramework.Repositories;
using SA.Identity.Domain.Entities;
using SA.Identity.Tests.Integration.Fixtures;
using Xunit;

namespace SA.Identity.Tests.Integration.Repositories;

public sealed class RoleRepositoryIntegrationTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid CreatedBy = Guid.NewGuid();

    public RoleRepositoryIntegrationTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    private static Role CreateRole(string name = "TestRole", Guid? tenantId = null)
    {
        return Role.Create(tenantId ?? TenantId, name, $"Description for {name}", CreatedBy);
    }

    /// <summary>
    /// TP-SEC-10-11: Role creation persists with permissions.
    /// </summary>
    [Fact]
    public async Task AddAsync_RolePersistsCorrectly()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var role = CreateRole();

        // Act
        await repo.AddAsync(role);
        await repo.SaveChangesAsync();

        // Assert
        var found = await repo.GetByIdAsync(role.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be("TestRole");
        found.IsSystem.Should().BeFalse();
    }

    /// <summary>
    /// TP-SEC-10-11: SetRolePermissionsAsync links permissions to role.
    /// </summary>
    [Fact]
    public async Task SetRolePermissionsAsync_LinksPermissionsToRole()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var role = CreateRole();
        await repo.AddAsync(role);

        var perm1 = Permission.Create("mod1", "act1", "code1", "desc1");
        var perm2 = Permission.Create("mod2", "act2", "code2", "desc2");
        ctx.Permissions.AddRange(perm1, perm2);
        await ctx.SaveChangesAsync();

        // Act
        await repo.SetRolePermissionsAsync(role.Id, new[] { perm1.Id, perm2.Id });
        await repo.SaveChangesAsync();

        // Assert
        var permIds = await repo.GetCurrentPermissionIdsAsync(role.Id);
        permIds.Should().HaveCount(2);
        permIds.Should().Contain(perm1.Id).And.Contain(perm2.Id);
    }

    /// <summary>
    /// TP-SEC-10-12: Diff-based permission update (DELETE removed, INSERT added).
    /// </summary>
    [Fact]
    public async Task SetRolePermissionsAsync_ReplacesPermissions_DiffBehavior()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var role = CreateRole();
        await repo.AddAsync(role);

        var perm1 = Permission.Create("m", "a1", "c1", null);
        var perm2 = Permission.Create("m", "a2", "c2", null);
        var perm3 = Permission.Create("m", "a3", "c3", null);
        ctx.Permissions.AddRange(perm1, perm2, perm3);
        await ctx.SaveChangesAsync();

        // Initial: perm1, perm2
        await repo.SetRolePermissionsAsync(role.Id, new[] { perm1.Id, perm2.Id });
        await repo.SaveChangesAsync();

        // Act: replace with perm2, perm3 (remove perm1, keep perm2, add perm3)
        await repo.SetRolePermissionsAsync(role.Id, new[] { perm2.Id, perm3.Id });
        await repo.SaveChangesAsync();

        // Assert
        var permIds = await repo.GetCurrentPermissionIdsAsync(role.Id);
        permIds.Should().HaveCount(2);
        permIds.Should().Contain(perm2.Id).And.Contain(perm3.Id);
        permIds.Should().NotContain(perm1.Id);
    }

    /// <summary>
    /// TP-SEC-10-13: Role deletion cascades role_permissions.
    /// </summary>
    [Fact]
    public async Task Delete_RemovesRoleAndCascadesPermissions()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var role = CreateRole();
        await repo.AddAsync(role);

        var perm = Permission.Create("m", "a", "c_del", null);
        ctx.Permissions.Add(perm);
        await ctx.SaveChangesAsync();

        await repo.SetRolePermissionsAsync(role.Id, new[] { perm.Id });
        await repo.SaveChangesAsync();

        // Act
        repo.Delete(role);
        await repo.SaveChangesAsync();

        // Assert
        var found = await repo.GetByIdAsync(role.Id);
        found.Should().BeNull();

        var remainingRolePerms = await ctx.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync();
        remainingRolePerms.Should().BeEmpty();
    }

    /// <summary>
    /// TP-SEC-09-09: List roles with search filter.
    /// </summary>
    [Fact]
    public async Task ListAsync_WithSearchFilter_ReturnsMatchingRoles()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var tenantId = Guid.NewGuid();
        var role1 = Role.Create(tenantId, "Administrator", "Full access", CreatedBy);
        var role2 = Role.Create(tenantId, "Operator", "Limited access", CreatedBy);
        var role3 = Role.Create(tenantId, "Viewer", "Read-only access", CreatedBy);
        await repo.AddAsync(role1);
        await repo.AddAsync(role2);
        await repo.AddAsync(role3);
        await repo.SaveChangesAsync();

        // Act
        var (items, total) = await repo.ListAsync(tenantId, 0, 10, search: "admin");

        // Assert
        total.Should().Be(1);
        items.Single().Name.Should().Be("Administrator");
    }

    /// <summary>
    /// TP-SEC-09-10: List roles with pagination.
    /// </summary>
    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var tenantId = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            var r = Role.Create(tenantId, $"Role_{i:D2}", $"Desc {i}", CreatedBy);
            await repo.AddAsync(r);
        }
        await repo.SaveChangesAsync();

        // Act
        var (items, total) = await repo.ListAsync(tenantId, 2, 2);

        // Assert
        total.Should().Be(5);
        items.Should().HaveCount(2);
    }

    /// <summary>
    /// TP-SEC-09-11: Permission counter is accessible via RolePermissions.Count.
    /// </summary>
    [Fact]
    public async Task GetByIdWithPermissionsAsync_IncludesPermissionCount()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var role = CreateRole("CountRole");
        await repo.AddAsync(role);

        var perm1 = Permission.Create("m1", "a1", "cnt_c1", null);
        var perm2 = Permission.Create("m2", "a2", "cnt_c2", null);
        var perm3 = Permission.Create("m3", "a3", "cnt_c3", null);
        ctx.Permissions.AddRange(perm1, perm2, perm3);
        await ctx.SaveChangesAsync();

        await repo.SetRolePermissionsAsync(role.Id, new[] { perm1.Id, perm2.Id, perm3.Id });
        await repo.SaveChangesAsync();

        // Act
        var found = await repo.GetByIdWithPermissionsAsync(role.Id);

        // Assert
        found.Should().NotBeNull();
        found!.RolePermissions.Should().HaveCount(3);
    }

    /// <summary>
    /// TP-SEC-09-12: User counter is accessible via UserRoles.Count.
    /// </summary>
    [Fact]
    public async Task GetByIdWithPermissionsAsync_IncludesUserCount()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var tenantId = Guid.NewGuid();
        var role = Role.Create(tenantId, "WithUsers", "desc", CreatedBy);
        await repo.AddAsync(role);

        var user1 = User.Create(tenantId, "u1", "h", "u1@t.com", "A", "B", null, null, CreatedBy);
        var user2 = User.Create(tenantId, "u2", "h", "u2@t.com", "C", "D", null, null, CreatedBy);
        ctx.Users.AddRange(user1, user2);
        await ctx.SaveChangesAsync();

        ctx.UserRoles.Add(UserRole.Create(user1.Id, role.Id));
        ctx.UserRoles.Add(UserRole.Create(user2.Id, role.Id));
        await ctx.SaveChangesAsync();

        // Act
        var found = await repo.GetByIdWithPermissionsAsync(role.Id);

        // Assert
        found.Should().NotBeNull();
        found!.UserRoles.Should().HaveCount(2);
    }

    /// <summary>
    /// TP-SEC-12-08: HasAssignedUsersAsync returns true when users exist.
    /// </summary>
    [Fact]
    public async Task HasAssignedUsersAsync_WithUsers_ReturnsTrue()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var tenantId = Guid.NewGuid();
        var role = Role.Create(tenantId, "AssignedRole", "desc", CreatedBy);
        await repo.AddAsync(role);

        var user = User.Create(tenantId, "assigned_user", "h", "au@t.com", "A", "B", null, null, CreatedBy);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        ctx.UserRoles.Add(UserRole.Create(user.Id, role.Id));
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.HasAssignedUsersAsync(role.Id);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// TP-SEC-12-09: HasAssignedUsersAsync returns false when no users.
    /// </summary>
    [Fact]
    public async Task HasAssignedUsersAsync_NoUsers_ReturnsFalse()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var role = CreateRole("NoUsersRole");
        await repo.AddAsync(role);
        await repo.SaveChangesAsync();

        // Act
        var result = await repo.HasAssignedUsersAsync(role.Id);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// TP-SEC-11-12: GetByNameAsync finds role by tenant and name.
    /// </summary>
    [Fact]
    public async Task GetByNameAsync_FindsRoleByTenantAndName()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var tenantId = Guid.NewGuid();
        var role = Role.Create(tenantId, "UniqueRole", "desc", CreatedBy);
        await repo.AddAsync(role);
        await repo.SaveChangesAsync();

        // Act
        var found = await repo.GetByNameAsync(tenantId, "UniqueRole");

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(role.Id);
    }

    /// <summary>
    /// TP-SEC-11-13: GetByNameAsync returns null for wrong tenant.
    /// </summary>
    [Fact]
    public async Task GetByNameAsync_WrongTenant_ReturnsNull()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RoleRepository(ctx);
        var tenantId = Guid.NewGuid();
        var role = Role.Create(tenantId, "TenantScopedRole", "desc", CreatedBy);
        await repo.AddAsync(role);
        await repo.SaveChangesAsync();

        // Act
        var found = await repo.GetByNameAsync(Guid.NewGuid(), "TenantScopedRole");

        // Assert
        found.Should().BeNull();
    }
}

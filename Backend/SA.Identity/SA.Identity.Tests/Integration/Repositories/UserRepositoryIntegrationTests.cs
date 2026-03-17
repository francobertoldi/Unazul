using FluentAssertions;
using SA.Identity.DataAccess.EntityFramework.Repositories;
using SA.Identity.Domain.Entities;
using SA.Identity.Tests.Integration.Fixtures;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Identity.Tests.Integration.Repositories;

public sealed class UserRepositoryIntegrationTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid CreatedBy = Guid.NewGuid();

    public UserRepositoryIntegrationTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    private static User CreateUser(
        string username = "john.doe",
        string email = "john@example.com",
        Guid? tenantId = null)
    {
        return User.Create(
            tenantId ?? TenantId,
            username,
            "hashed_password",
            email,
            "John",
            "Doe",
            null,
            null,
            CreatedBy);
    }

    /// <summary>
    /// TP-SEC-07-12: Username unique constraint per tenant (tenant_id, username).
    /// InMemory does not enforce unique indexes, so we test via GetByUsernameAsync lookup.
    /// </summary>
    [Fact]
    public async Task GetByUsernameAsync_FindsUserByTenantAndUsername()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var user = CreateUser();
        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        // Act
        var found = await repo.GetByUsernameAsync(TenantId, "john.doe");

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
    }

    /// <summary>
    /// TP-SEC-07-13: Same username in different tenants does not conflict.
    /// </summary>
    [Fact]
    public async Task GetByUsernameAsync_SameUsernameInDifferentTenants_ReturnsCorrectUser()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var user1 = CreateUser("shared.name", "u1@a.com", tenant1);
        var user2 = CreateUser("shared.name", "u2@b.com", tenant2);
        await repo.AddAsync(user1);
        await repo.AddAsync(user2);
        await repo.SaveChangesAsync();

        // Act
        var found1 = await repo.GetByUsernameAsync(tenant1, "shared.name");
        var found2 = await repo.GetByUsernameAsync(tenant2, "shared.name");

        // Assert
        found1.Should().NotBeNull();
        found2.Should().NotBeNull();
        found1!.Id.Should().NotBe(found2!.Id);
    }

    /// <summary>
    /// TP-SEC-06-08: Email unique constraint per tenant (tenant_id, email).
    /// </summary>
    [Fact]
    public async Task GetByEmailAsync_FindsUserByTenantAndEmail()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var user = CreateUser();
        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        // Act
        var found = await repo.GetByEmailAsync(TenantId, "john@example.com");

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
    }

    /// <summary>
    /// TP-SEC-06-09: Email lookup in wrong tenant returns null.
    /// </summary>
    [Fact]
    public async Task GetByEmailAsync_WrongTenant_ReturnsNull()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var user = CreateUser();
        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        // Act
        var found = await repo.GetByEmailAsync(Guid.NewGuid(), "john@example.com");

        // Assert
        found.Should().BeNull();
    }

    /// <summary>
    /// TP-SEC-06-08: List users with search filter matches on username, email, first/last name.
    /// </summary>
    [Fact]
    public async Task ListAsync_WithSearchFilter_ReturnsMatchingUsers()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var tenantId = Guid.NewGuid();
        var user1 = User.Create(tenantId, "alice", "h", "alice@test.com", "Alice", "Smith", null, null, CreatedBy);
        var user2 = User.Create(tenantId, "bob", "h", "bob@test.com", "Bob", "Johnson", null, null, CreatedBy);
        var user3 = User.Create(tenantId, "charlie", "h", "charlie@test.com", "Charlie", "Smith", null, null, CreatedBy);
        await repo.AddAsync(user1);
        await repo.AddAsync(user2);
        await repo.AddAsync(user3);
        await repo.SaveChangesAsync();

        // Act - search for "Smith" (should match Alice and Charlie by last name)
        var (items, total) = await repo.ListAsync(tenantId, 0, 10, search: "smith");

        // Assert
        total.Should().Be(2);
        items.Should().HaveCount(2);
        items.Select(u => u.Username).Should().Contain("alice").And.Contain("charlie");
    }

    /// <summary>
    /// TP-SEC-06-08: List users with status filter.
    /// </summary>
    [Fact]
    public async Task ListAsync_WithStatusFilter_ReturnsOnlyMatchingStatus()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var tenantId = Guid.NewGuid();
        var activeUser = User.Create(tenantId, "active.user", "h", "active@test.com", "Active", "User", null, null, CreatedBy);
        var inactiveUser = User.Create(tenantId, "inactive.user", "h", "inactive@test.com", "Inactive", "User", null, null, CreatedBy);
        inactiveUser.Update("inactive@test.com", "Inactive", "User", null, null, UserStatus.Inactive, null, CreatedBy);

        await repo.AddAsync(activeUser);
        await repo.AddAsync(inactiveUser);
        await repo.SaveChangesAsync();

        // Act
        var (items, total) = await repo.ListAsync(tenantId, 0, 10, status: UserStatus.Active);

        // Assert
        total.Should().Be(1);
        items.Single().Username.Should().Be("active.user");
    }

    /// <summary>
    /// TP-SEC-01-11: List users with pagination.
    /// </summary>
    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var tenantId = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            var u = User.Create(tenantId, $"user{i:D2}", "h", $"user{i}@test.com", "User", $"N{i}", null, null, CreatedBy);
            await repo.AddAsync(u);
        }
        await repo.SaveChangesAsync();

        // Act - second page with page size 2
        var (items, total) = await repo.ListAsync(tenantId, 2, 2);

        // Assert
        total.Should().Be(5);
        items.Should().HaveCount(2);
    }

    /// <summary>
    /// TP-SEC-07-15: User creation persists correctly.
    /// </summary>
    [Fact]
    public async Task AddAsync_UserPersistsCorrectly()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var user = CreateUser();

        // Act
        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        // Assert
        var found = await repo.GetByIdAsync(user.Id);
        found.Should().NotBeNull();
        found!.Username.Should().Be("john.doe");
        found.Email.Should().Be("john@example.com");
        found.FirstName.Should().Be("John");
        found.LastName.Should().Be("Doe");
        found.IsActive.Should().BeTrue();
        found.FailedLoginAttempts.Should().Be(0);
    }

    /// <summary>
    /// TP-SEC-07-15: SetUserRolesAsync replaces user roles.
    /// </summary>
    [Fact]
    public async Task SetUserRolesAsync_ReplacesExistingRoles()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var user = CreateUser();
        await repo.AddAsync(user);

        var role1 = Role.Create(TenantId, "Role1", "desc", CreatedBy);
        var role2 = Role.Create(TenantId, "Role2", "desc", CreatedBy);
        var role3 = Role.Create(TenantId, "Role3", "desc", CreatedBy);
        ctx.Roles.AddRange(role1, role2, role3);
        await ctx.SaveChangesAsync();

        // Set initial roles
        await repo.SetUserRolesAsync(user.Id, new[] { role1.Id, role2.Id });
        await repo.SaveChangesAsync();

        // Act - replace with different roles
        await repo.SetUserRolesAsync(user.Id, new[] { role2.Id, role3.Id });
        await repo.SaveChangesAsync();

        // Assert
        var userRoles = ctx.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
        userRoles.Should().HaveCount(2);
        userRoles.Select(ur => ur.RoleId).Should().Contain(role2.Id).And.Contain(role3.Id);
        userRoles.Select(ur => ur.RoleId).Should().NotContain(role1.Id);
    }

    /// <summary>
    /// TP-SEC-07-15: SetUserAssignmentsAsync replaces existing assignments.
    /// </summary>
    [Fact]
    public async Task SetUserAssignmentsAsync_ReplacesExistingAssignments()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var user = CreateUser();
        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        var assignment1 = UserAssignment.Create(user.Id, "branch", Guid.NewGuid(), "Branch 1");
        await repo.SetUserAssignmentsAsync(user.Id, new[] { assignment1 });
        await repo.SaveChangesAsync();

        // Act - replace with a different assignment
        var assignment2 = UserAssignment.Create(user.Id, "branch", Guid.NewGuid(), "Branch 2");
        await repo.SetUserAssignmentsAsync(user.Id, new[] { assignment2 });
        await repo.SaveChangesAsync();

        // Assert
        var assignments = ctx.UserAssignments.Where(a => a.UserId == user.Id).ToList();
        assignments.Should().HaveCount(1);
        assignments.Single().ScopeName.Should().Be("Branch 2");
    }

    /// <summary>
    /// TP-SEC-07-12: User update persists changes.
    /// </summary>
    [Fact]
    public async Task Update_UserPersistsChanges()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new UserRepository(ctx);
        var user = CreateUser();
        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        // Act
        user.Update("new@email.com", "Jane", "Smith", null, null, UserStatus.Inactive, "avatar.png", CreatedBy);
        repo.Update(user);
        await repo.SaveChangesAsync();

        // Assert
        var found = await repo.GetByIdAsync(user.Id);
        found.Should().NotBeNull();
        found!.Email.Should().Be("new@email.com");
        found.FirstName.Should().Be("Jane");
        found.LastName.Should().Be("Smith");
        found.Status.Should().Be(UserStatus.Inactive);
        found.Avatar.Should().Be("avatar.png");
    }
}

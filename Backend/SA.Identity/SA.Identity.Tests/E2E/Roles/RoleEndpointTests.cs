using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SA.Identity.Api.ViewModels.Auth.Login;
using SA.Identity.Api.ViewModels.Roles;
using SA.Identity.DataAccess.EntityFramework.Persistence;
using SA.Identity.DataAccess.EntityFramework.Seed;
using SA.Identity.Tests.E2E.Fixtures;
using SA.Identity.Tests.E2E.Users;
using Xunit;

namespace SA.Identity.Tests.E2E.Roles;

public sealed class RoleEndpointTests : IClassFixture<IdentityWebAppFactory>
{
    private readonly IdentityWebAppFactory _factory;

    public RoleEndpointTests(IdentityWebAppFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(IdentityWebAppFactory? factory = null)
    {
        var f = factory ?? _factory;
        var client = f.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", IdentityWebAppFactory.TestTenantId.ToString());

        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(IdentityWebAppFactory.AdminUsername, IdentityWebAppFactory.AdminPassword));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        return client;
    }

    private static Guid[] GetSomePermissionIds(int count = 3)
    {
        var definitions = PermissionSeedData.GetDefinitions();
        return definitions
            .Take(count)
            .Select(d => PermissionSeedData.GetPermissionId(d.Code))
            .ToArray();
    }

    /// <summary>
    /// TP-SEC-09-13: Create role -> list roles -> verify role appears with counters.
    /// </summary>
    [Fact]
    public async Task CreateRole_ThenListRoles_VerifyCounters()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = await CreateAuthenticatedClientAsync(factory);
        var permIds = GetSomePermissionIds(5);

        var createRequest = new CreateRoleRequest("TestOperator", "Operator for tests", permIds);

        // Act - create role
        var createResponse = await client.PostAsJsonAsync("/api/v1/roles", createRequest);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // List roles and find the created one
        var listResponse = await client.GetAsync("/api/v1/roles?page=1&page_size=50&search=TestOperator");
        listResponse.EnsureSuccessStatusCode();
        var listBody = await listResponse.Content.ReadFromJsonAsync<RoleListResponseDto>();
        listBody.Should().NotBeNull();

        var createdRole = listBody!.Items.FirstOrDefault(r => r.Name == "TestOperator");
        createdRole.Should().NotBeNull();
        createdRole!.PermissionCount.Should().Be(5);
        createdRole.UserCount.Should().Be(0);
        createdRole.IsSystem.Should().BeFalse();
    }

    /// <summary>
    /// TP-SEC-10-15: Create role -> edit permissions -> verify diff-based update.
    /// </summary>
    [Fact]
    public async Task CreateRole_EditPermissions_VerifyDiff()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = await CreateAuthenticatedClientAsync(factory);
        var initialPerms = GetSomePermissionIds(3);

        var createRequest = new CreateRoleRequest("DiffRole", "For diff test", initialPerms);
        var createResponse = await client.PostAsJsonAsync("/api/v1/roles", createRequest);
        createResponse.EnsureSuccessStatusCode();

        // Read back the created role to get its ID
        var createdBody = await createResponse.Content.ReadFromJsonAsync<RoleDetailResponseDto>();
        var roleId = createdBody!.Id;

        // Act - update with different permissions (keep first, drop second+third, add two new)
        var newPerms = new[]
        {
            initialPerms[0], // keep
            GetSomePermissionIds(10).Skip(5).Take(1).First(), // new
            GetSomePermissionIds(10).Skip(6).Take(1).First()  // new
        };

        var updateRequest = new UpdateRoleRequest("DiffRole", "Updated description", newPerms);
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/roles/{roleId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedBody = await updateResponse.Content.ReadFromJsonAsync<RoleDetailResponseDto>();
        updatedBody.Should().NotBeNull();
        updatedBody!.Permissions.Should().HaveCount(newPerms.Distinct().Count());
    }

    /// <summary>
    /// TP-SEC-11-16: Create role -> delete -> verify gone.
    /// </summary>
    [Fact]
    public async Task CreateRole_ThenDelete_VerifyGone()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = await CreateAuthenticatedClientAsync(factory);
        var permIds = GetSomePermissionIds(2);

        var createRequest = new CreateRoleRequest("ToDelete", "Will be deleted", permIds);
        var createResponse = await client.PostAsJsonAsync("/api/v1/roles", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createdBody = await createResponse.Content.ReadFromJsonAsync<RoleDetailResponseDto>();
        var roleId = createdBody!.Id;

        // Act - delete the role
        var deleteResponse = await client.DeleteAsync($"/api/v1/roles/{roleId}");

        // Assert - delete succeeds
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's gone from the list
        var listResponse = await client.GetAsync("/api/v1/roles?page=1&page_size=100&search=ToDelete");
        listResponse.EnsureSuccessStatusCode();
        var listBody = await listResponse.Content.ReadFromJsonAsync<RoleListResponseDto>();
        listBody!.Items.Should().NotContain(r => r.Name == "ToDelete");
    }

    /// <summary>
    /// TP-SEC-12-10: System role cannot be deleted.
    /// </summary>
    [Fact]
    public async Task DeleteSystemRole_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = await CreateAuthenticatedClientAsync(factory);

        // Find the Super Admin role (system role)
        var listResponse = await client.GetAsync("/api/v1/roles?page=1&page_size=50");
        listResponse.EnsureSuccessStatusCode();
        var listBody = await listResponse.Content.ReadFromJsonAsync<RoleListResponseDto>();
        var superAdminRole = listBody!.Items.First(r => r.Name == "Super Admin");

        // Act - try to delete the system role
        var deleteResponse = await client.DeleteAsync($"/api/v1/roles/{superAdminRole.Id}");

        // Assert - should fail (system role protection)
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// TP-SEC-09-14: Roles endpoint requires authentication.
    /// </summary>
    [Fact]
    public async Task ListRoles_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/roles?page=1&page_size=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// TP-SEC-10-16: Create role without permissions returns 422.
    /// </summary>
    [Fact]
    public async Task CreateRole_EmptyPermissions_Returns422()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = await CreateAuthenticatedClientAsync(factory);

        var request = new CreateRoleRequest("EmptyPermsRole", "No perms", Array.Empty<Guid>());

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be((HttpStatusCode)422);
    }

    /// <summary>
    /// TP-SEC-10-17: Create role with nonexistent permission ID returns 422.
    /// </summary>
    [Fact]
    public async Task CreateRole_NonExistentPermission_Returns422()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = await CreateAuthenticatedClientAsync(factory);

        var request = new CreateRoleRequest("BadPermsRole", "Bad perms",
            new[] { Guid.NewGuid() });

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be((HttpStatusCode)422);
    }

    /// <summary>
    /// TP-SEC-10-18: Create role with duplicate name returns 400.
    /// </summary>
    [Fact]
    public async Task CreateRole_DuplicateName_Returns400()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = await CreateAuthenticatedClientAsync(factory);
        var permIds = GetSomePermissionIds(2);

        // "Super Admin" is already seeded
        var request = new CreateRoleRequest("Super Admin", "Duplicate", permIds);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// TP-SEC-12-11: Delete role that has assigned users returns 400.
    /// </summary>
    [Fact]
    public async Task DeleteRole_WithAssignedUsers_Returns400()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = await CreateAuthenticatedClientAsync(factory);

        // The Super Admin role has the admin user assigned to it.
        // But it's also a system role, so the system check fires first.
        // Let's create a custom role, assign a user to it, then try to delete.
        var permIds = GetSomePermissionIds(2);
        var createRoleReq = new CreateRoleRequest("AssignedUsersRole", "Has users", permIds);
        var createRoleResp = await client.PostAsJsonAsync("/api/v1/roles", createRoleReq);
        createRoleResp.EnsureSuccessStatusCode();
        var role = await createRoleResp.Content.ReadFromJsonAsync<RoleDetailResponseDto>();

        // Create a user assigned to this role
        var createUserReq = new SA.Identity.Api.ViewModels.Users.CreateUserRequest(
            "roleuser",
            "P@ssw0rd!",
            "roleuser@test.com",
            "Role",
            "User",
            null,
            null,
            new[] { role!.Id },
            Array.Empty<SA.Identity.Api.ViewModels.Users.CreateUserAssignmentRequest>());
        var createUserResp = await client.PostAsJsonAsync("/api/v1/users", createUserReq);
        createUserResp.EnsureSuccessStatusCode();

        // Act - try to delete the role that has assigned users
        var deleteResponse = await client.DeleteAsync($"/api/v1/roles/{role.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

// Internal DTOs for deserialization
internal sealed record RoleDetailResponseDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<RolePermissionItemDto> Permissions);

internal sealed record RolePermissionItemDto(
    Guid Id,
    string Module,
    string Action,
    string Code,
    string? Description);

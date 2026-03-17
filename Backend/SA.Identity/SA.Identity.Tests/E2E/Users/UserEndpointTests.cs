using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Identity.Api.ViewModels.Auth.Login;
using SA.Identity.Api.ViewModels.Users;
using SA.Identity.Tests.E2E.Fixtures;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Identity.Tests.E2E.Users;

public sealed class UserEndpointTests : IClassFixture<IdentityWebAppFactory>
{
    private readonly IdentityWebAppFactory _factory;

    public UserEndpointTests(IdentityWebAppFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient Client, LoginResponse Login)> CreateAuthenticatedClientAsync(
        IdentityWebAppFactory? factory = null)
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

        return (client, login);
    }

    /// <summary>
    /// TP-SEC-06-11: Create user -> list users -> find created user.
    /// </summary>
    [Fact]
    public async Task CreateUser_ThenListUsers_FindsCreatedUser()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var (client, _) = await CreateAuthenticatedClientAsync(factory);

        // Get a role ID from the seeded Super Admin role
        var listRolesResponse = await client.GetAsync("/api/v1/roles?page=1&page_size=10");
        listRolesResponse.EnsureSuccessStatusCode();
        var rolesBody = await listRolesResponse.Content.ReadFromJsonAsync<RoleListResponseDto>();
        var superAdminRoleId = rolesBody!.Items.First(r => r.Name == "Super Admin").Id;

        var createRequest = new CreateUserRequest(
            "newuser.test",
            "P@ssw0rd!",
            "newuser@test.com",
            "New",
            "User",
            null,
            null,
            new[] { superAdminRoleId },
            Array.Empty<CreateUserAssignmentRequest>());

        // Act - create user
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", createRequest);

        // Assert - creation succeeds
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        createdUser.Should().NotBeNull();
        createdUser!.Username.Should().Be("newuser.test");
        createdUser.Email.Should().Be("newuser@test.com");

        // Act - list users and find the newly created one
        var listResponse = await client.GetAsync("/api/v1/users?page=1&page_size=50&search=newuser.test");
        listResponse.EnsureSuccessStatusCode();
        var listBody = await listResponse.Content.ReadFromJsonAsync<UserListResponseDto>();

        // Assert
        listBody.Should().NotBeNull();
        listBody!.Items.Should().Contain(u => u.Username == "newuser.test");
    }

    /// <summary>
    /// TP-SEC-06-12: Create user -> get detail -> verify effective permissions.
    /// </summary>
    [Fact]
    public async Task CreateUser_GetDetail_VerifyEffectivePermissions()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var (client, _) = await CreateAuthenticatedClientAsync(factory);

        var listRolesResponse = await client.GetAsync("/api/v1/roles?page=1&page_size=10");
        var rolesBody = await listRolesResponse.Content.ReadFromJsonAsync<RoleListResponseDto>();
        var superAdminRoleId = rolesBody!.Items.First(r => r.Name == "Super Admin").Id;

        var createRequest = new CreateUserRequest(
            "detail.user",
            "P@ssw0rd!",
            "detail@test.com",
            "Detail",
            "User",
            null,
            null,
            new[] { superAdminRoleId },
            Array.Empty<CreateUserAssignmentRequest>());

        var createResponse = await client.PostAsJsonAsync("/api/v1/users", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<UserDetailResponse>();

        // Act - get user detail
        var detailResponse = await client.GetAsync($"/api/v1/users/{created!.Id}");

        // Assert
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await detailResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        detail.Should().NotBeNull();
        detail!.Username.Should().Be("detail.user");
        detail.Roles.Should().Contain(r => r.Name == "Super Admin");
        detail.EffectivePermissions.Should().NotBeNullOrEmpty();
        detail.EffectivePermissions!.Count.Should().Be(
            SA.Identity.DataAccess.EntityFramework.Seed.PermissionSeedData.Count);
    }

    /// <summary>
    /// TP-SEC-07-16: Update user -> verify changes.
    /// </summary>
    [Fact]
    public async Task UpdateUser_VerifyChanges()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var (client, _) = await CreateAuthenticatedClientAsync(factory);

        var listRolesResponse = await client.GetAsync("/api/v1/roles?page=1&page_size=10");
        var rolesBody = await listRolesResponse.Content.ReadFromJsonAsync<RoleListResponseDto>();
        var superAdminRoleId = rolesBody!.Items.First(r => r.Name == "Super Admin").Id;

        // Create a user first
        var createRequest = new CreateUserRequest(
            "update.user",
            "P@ssw0rd!",
            "update@test.com",
            "Before",
            "Update",
            null,
            null,
            new[] { superAdminRoleId },
            Array.Empty<CreateUserAssignmentRequest>());

        var createResponse = await client.PostAsJsonAsync("/api/v1/users", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<UserDetailResponse>();

        // Act - update the user
        var updateRequest = new UpdateUserRequest(
            "updated@test.com",
            "After",
            "Update",
            null,
            null,
            UserStatus.Active,
            "new-avatar.png",
            new[] { superAdminRoleId },
            Array.Empty<UpdateUserAssignmentRequest>());

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/users/{created!.Id}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        updated.Should().NotBeNull();
        updated!.Email.Should().Be("updated@test.com");
        updated.FirstName.Should().Be("After");
        updated.Avatar.Should().Be("new-avatar.png");
    }

    /// <summary>
    /// TP-SEC-07-17: Get user detail for non-existent user returns 404.
    /// </summary>
    [Fact]
    public async Task GetUserDetail_NonExistentUser_Returns404()
    {
        // Arrange
        var (client, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// TP-SEC-08-08: Users endpoint requires authentication.
    /// </summary>
    [Fact]
    public async Task ListUsers_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/users?page=1&page_size=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// TP-SEC-08-09: Create user endpoint requires authentication.
    /// </summary>
    [Fact]
    public async Task CreateUser_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateUserRequest(
            "unauth", "P@ss!", "u@t.com", "A", "B", null, null,
            Array.Empty<Guid>(), Array.Empty<CreateUserAssignmentRequest>());

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

// Internal DTOs for deserialization (to avoid coupling to internal response shapes)
internal sealed record RoleListResponseDto(
    IReadOnlyList<RoleListItemDto> Items,
    int Total,
    int Page,
    int PageSize);

internal sealed record RoleListItemDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    int PermissionCount,
    int UserCount);

internal sealed record UserListResponseDto(
    IReadOnlyList<UserListItemDto> Items,
    int Total,
    int Page,
    int PageSize);

internal sealed record UserListItemDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? Avatar,
    IReadOnlyList<string>? Roles);

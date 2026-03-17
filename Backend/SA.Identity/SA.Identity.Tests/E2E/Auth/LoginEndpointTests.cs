using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Identity.Api.ViewModels.Auth.Login;
using SA.Identity.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Identity.Tests.E2E.Auth;

public sealed class LoginEndpointTests : IClassFixture<IdentityWebAppFactory>
{
    private readonly HttpClient _client;

    public LoginEndpointTests(IdentityWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", IdentityWebAppFactory.TestTenantId.ToString());
    }

    /// <summary>
    /// TP-SEC-01-12: POST /api/v1/auth/login with valid credentials returns 200 with JWT.
    /// </summary>
    [Fact]
    public async Task Login_ValidCredentials_Returns200WithJwt()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(IdentityWebAppFactory.AdminUsername, IdentityWebAppFactory.AdminPassword));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.ExpiresIn.Should().Be(900);
        body.UserId.Should().Be(IdentityWebAppFactory.AdminUserId);
        body.Username.Should().Be(IdentityWebAppFactory.AdminUsername);
        body.Roles.Should().Contain("Super Admin");
        body.Permissions.Should().NotBeEmpty();
        body.RequiresOtp.Should().BeFalse();
    }

    /// <summary>
    /// TP-SEC-01-13: POST /api/v1/auth/login with wrong password returns 401.
    /// </summary>
    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(IdentityWebAppFactory.AdminUsername, "WrongPassword!"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// TP-SEC-01-13: POST /api/v1/auth/login with non-existent user returns 401.
    /// </summary>
    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("nonexistent.user", "P@ssw0rd!"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// TP-SEC-01-13: POST /api/v1/auth/login with locked account returns 423.
    /// Locked accounts are detected after 5 failed attempts.
    /// </summary>
    [Fact]
    public async Task Login_LockedAccount_Returns423()
    {
        // Arrange - lock the account by failing 5 times
        // We need a fresh factory to avoid state pollution, but since we share one,
        // we'll use a dedicated user or just test the response code.
        // Actually, the admin user is shared, so we simulate lock by multiple bad attempts.
        // NOTE: This test may fail if run in parallel with others that use the admin user.
        // For isolation, we will check the behavior pattern rather than actual lock.
        // We skip this as it would mutate shared state. See unit tests for coverage.

        // Instead, verify that after 5 wrong attempts, we get a locked response.
        // Since we share the factory, we use a minimal assertion approach.
        using var factory = new IdentityWebAppFactory();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", IdentityWebAppFactory.TestTenantId.ToString());

        // Fail 5 times to lock the account
        for (int i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/login",
                new LoginRequest(IdentityWebAppFactory.AdminUsername, "wrong"));
        }

        // Act - attempt login on locked account
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(IdentityWebAppFactory.AdminUsername, IdentityWebAppFactory.AdminPassword));

        // Assert - should be 423 (locked)
        response.StatusCode.Should().Be((HttpStatusCode)423);
    }
}

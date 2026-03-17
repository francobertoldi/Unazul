using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Identity.Api.ViewModels.Auth.Login;
using SA.Identity.Api.ViewModels.Auth.Logout;
using SA.Identity.Api.ViewModels.Auth.Refresh;
using SA.Identity.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Identity.Tests.E2E.Auth;

public sealed class LogoutEndpointTests : IClassFixture<IdentityWebAppFactory>
{
    private readonly IdentityWebAppFactory _factory;

    public LogoutEndpointTests(IdentityWebAppFactory factory)
    {
        _factory = factory;
    }

    private async Task<LoginResponse> LoginAsync(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", IdentityWebAppFactory.TestTenantId.ToString());

        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(IdentityWebAppFactory.AdminUsername, IdentityWebAppFactory.AdminPassword));

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
    }

    /// <summary>
    /// TP-SEC-05-09: Logout flow: login -> logout -> try refresh -> 401.
    /// </summary>
    [Fact]
    public async Task Logout_ThenRefresh_Returns401()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = factory.CreateClient();
        var loginResult = await LoginAsync(client);

        // Authenticate for logout
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        // Act - logout
        var logoutResponse = await client.PostAsJsonAsync("/api/v1/auth/logout",
            new LogoutRequest(loginResult.RefreshToken));

        // Assert - logout succeeds
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Remove auth header for refresh attempt
        client.DefaultRequestHeaders.Authorization = null;

        // Act - try to refresh with the revoked token
        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(loginResult.RefreshToken!));

        // Assert - refresh should fail
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// TP-SEC-05-10: Logout without authentication returns 401.
    /// </summary>
    [Fact]
    public async Task Logout_WithoutAuthentication_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - try to logout without auth
        var response = await client.PostAsJsonAsync("/api/v1/auth/logout",
            new LogoutRequest(null));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// TP-SEC-05-09: Logout with null refresh token revokes all tokens and succeeds.
    /// </summary>
    [Fact]
    public async Task Logout_WithNullRefreshToken_Succeeds()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = factory.CreateClient();
        var loginResult = await LoginAsync(client);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        // Act - logout without specifying a refresh token
        var response = await client.PostAsJsonAsync("/api/v1/auth/logout",
            new LogoutRequest(null));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

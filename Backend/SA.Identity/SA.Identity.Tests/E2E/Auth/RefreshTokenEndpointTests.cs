using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Identity.Api.ViewModels.Auth.Login;
using SA.Identity.Api.ViewModels.Auth.Refresh;
using SA.Identity.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Identity.Tests.E2E.Auth;

public sealed class RefreshTokenEndpointTests : IClassFixture<IdentityWebAppFactory>
{
    private readonly IdentityWebAppFactory _factory;

    public RefreshTokenEndpointTests(IdentityWebAppFactory factory)
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
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!;
    }

    /// <summary>
    /// TP-SEC-03-09: Refresh flow: login -> get refresh token -> POST /refresh -> new JWT.
    /// </summary>
    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNewJwt()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = factory.CreateClient();
        var loginResult = await LoginAsync(client);

        // Act
        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(loginResult.RefreshToken!));

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.ExpiresIn.Should().Be(900);

        // New tokens should be different from original
        body.AccessToken.Should().NotBe(loginResult.AccessToken);
        body.RefreshToken.Should().NotBe(loginResult.RefreshToken);
    }

    /// <summary>
    /// TP-SEC-03-10: Refresh with invalid/nonexistent token returns 401.
    /// </summary>
    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest("nonexistent-refresh-token"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// TP-SEC-03-10: Refresh with already-used (revoked) token returns 401.
    /// </summary>
    [Fact]
    public async Task Refresh_WithAlreadyUsedToken_Returns401()
    {
        // Arrange
        using var factory = new IdentityWebAppFactory();
        var client = factory.CreateClient();
        var loginResult = await LoginAsync(client);

        // Use the refresh token once (this revokes it)
        await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(loginResult.RefreshToken!));

        // Act - try to use the same refresh token again (reuse detection)
        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(loginResult.RefreshToken!));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

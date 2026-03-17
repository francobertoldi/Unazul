using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SA.Audit.DataAccess.EntityFramework.Persistence;
using SA.Audit.Domain.Entities;
using SA.Audit.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Audit.Tests.E2E;

public sealed class AuditLogEndpointsTests : IClassFixture<AuditWebAppFactory>
{
    private readonly AuditWebAppFactory _factory;
    private readonly HttpClient _unauthenticatedClient;

    private static readonly Guid TestTenantId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    // Must match appsettings.json Jwt:Key / Jwt:Issuer / Jwt:Audience
    private const string JwtKey = "super-secret-key-for-development-only-min-32-chars!!";
    private const string JwtIssuer = "sa-identity";
    private const string JwtAudience = "unazul-backoffice";

    public AuditLogEndpointsTests(AuditWebAppFactory factory)
    {
        _factory = factory;
        _unauthenticatedClient = factory.CreateClient();
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestJwt());
        return client;
    }

    private static string GenerateTestJwt()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, TestUserId.ToString()),
            new Claim("tenant_id", TestTenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task SeedAuditLogs(int count = 5)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        for (int i = 0; i < count; i++)
        {
            db.AuditLogs.Add(AuditLog.Create(
                TestTenantId,
                TestUserId,
                "admin",
                "Crear",
                "Usuarios",
                $"Action_{i}",
                $"Detail {i}",
                "127.0.0.1",
                "User",
                Guid.NewGuid(),
                null,
                DateTimeOffset.UtcNow.AddMinutes(-i)));
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task TP_AUD_01_E2E_List_Returns_200_With_Paginated_Items()
    {
        // Arrange
        await SeedAuditLogs(3);
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/v1/audit-log");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"items\"");
        content.Should().Contain("\"total\"");
    }

    [Fact]
    public async Task TP_AUD_06_E2E_List_With_Combined_Filters()
    {
        // Arrange
        await SeedAuditLogs(2);
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/audit-log?user_id={TestUserId}&operation=Crear&module=Usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TP_AUD_09_E2E_List_Without_Auth_Returns_401()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/audit-log");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_AUD_22_E2E_Export_Xlsx_Returns_File()
    {
        // Arrange
        await SeedAuditLogs(2);
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/v1/audit-log/export?format=xlsx");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public async Task TP_AUD_23_E2E_Export_Csv_Returns_File()
    {
        // Arrange
        await SeedAuditLogs(2);
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/v1/audit-log/export?format=csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType
            .Should().Be("text/csv");
    }

    [Fact]
    public async Task TP_AUD_28_E2E_Export_Invalid_Format_Returns_422()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/v1/audit-log/export?format=pdf");

        // Assert
        ((int)response.StatusCode).Should().Be(422);
    }
}

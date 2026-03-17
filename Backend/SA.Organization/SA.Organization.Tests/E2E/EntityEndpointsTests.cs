using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Organization.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Organization.Tests.E2E;

public sealed class EntityEndpointsTests : IClassFixture<OrganizationWebAppFactory>
{
    private readonly HttpClient _client;

    public EntityEndpointsTests(OrganizationWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TP_ORG_06_08_ListEntities_WithoutJwt_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/entities");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_07_15_CreateEntity_WithoutJwt_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/entities", new { TenantId = Guid.NewGuid(), Name = "Test", Cuit = "20-12345678-1", Type = "Bank", Status = "Active" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_07_15b_UpdateEntity_WithoutJwt_Returns401()
    {
        var id = Guid.NewGuid();
        var response = await _client.PutAsJsonAsync($"/api/v1/entities/{id}", new { Name = "Test", Type = "Bank", Status = "Active" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_08_08_GetEntityDetail_WithoutJwt_Returns401()
    {
        var id = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/entities/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_09_06_DeleteEntity_WithoutJwt_Returns401()
    {
        var id = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/v1/entities/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_06_05_ExportEntities_WithoutJwt_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/entities/export?format=xlsx");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

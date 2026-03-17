using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Organization.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Organization.Tests.E2E;

public sealed class TenantEndpointsTests : IClassFixture<OrganizationWebAppFactory>
{
    private readonly HttpClient _client;

    public TenantEndpointsTests(OrganizationWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TP_ORG_01_07_ListTenants_WithoutJwt_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/tenants");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_02_08_CreateTenant_WithoutJwt_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/tenants", new { Name = "Test", Identifier = "20-12345678-1", Status = "Active" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_03_08_UpdateTenant_WithoutJwt_Returns401()
    {
        var id = Guid.NewGuid();
        var response = await _client.PutAsJsonAsync($"/api/v1/tenants/{id}", new { Name = "Test", Status = "Active" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_04_06_GetTenantDetail_WithoutJwt_Returns401()
    {
        var id = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/tenants/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_05_05_DeleteTenant_WithoutJwt_Returns401()
    {
        var id = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/v1/tenants/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_01_04_ExportTenants_WithoutJwt_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/tenants/export?format=xlsx");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

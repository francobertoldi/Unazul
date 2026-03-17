using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Organization.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Organization.Tests.E2E;

public sealed class BranchEndpointsTests : IClassFixture<OrganizationWebAppFactory>
{
    private readonly HttpClient _client;

    public BranchEndpointsTests(OrganizationWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TP_ORG_10_13_ListBranches_WithoutJwt_Returns401()
    {
        var entityId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/entities/{entityId}/branches");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_10_13b_CreateBranch_WithoutJwt_Returns401()
    {
        var entityId = Guid.NewGuid();
        var response = await _client.PostAsJsonAsync($"/api/v1/entities/{entityId}/branches", new { Name = "Test", Code = "BR-001", IsActive = true });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_10_13c_UpdateBranch_WithoutJwt_Returns401()
    {
        var entityId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var response = await _client.PutAsJsonAsync($"/api/v1/entities/{entityId}/branches/{branchId}", new { Name = "Test", IsActive = true });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TP_ORG_10_13d_DeleteBranch_WithoutJwt_Returns401()
    {
        var entityId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/v1/entities/{entityId}/branches/{branchId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

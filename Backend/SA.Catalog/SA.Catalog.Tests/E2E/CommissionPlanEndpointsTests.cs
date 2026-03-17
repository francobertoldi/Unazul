using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SA.Catalog.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Catalog.Tests.E2E;

public sealed class CommissionPlanEndpointsTests : IClassFixture<CatalogWebAppFactory>
{
    private readonly CatalogWebAppFactory _factory;
    private readonly HttpClient _client;

    private static readonly string[] AllPermissions =
    [
        "p_cat_list", "p_cat_create", "p_cat_edit", "p_cat_delete", "p_cat_detail",
        "p_cat_commissions_list", "p_cat_commissions_create", "p_cat_commissions_edit", "p_cat_commissions_delete"
    ];

    public CommissionPlanEndpointsTests(CatalogWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(AllPermissions);
    }

    private async Task<Guid> CreateTestCommissionPlan(string code = "COM-001")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/commission-plans", new
        {
            Code = code,
            Description = "Plan comision test",
            Type = "PercentageCapital",
            Value = 5.0m,
            MaxAmount = 10000.0m
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task TP_CAT_E2E_11_CreateCommissionPlan_Returns201()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/commission-plans", new
        {
            Code = "COM-E2E-001",
            Description = "Plan comision E2E",
            Type = "FixedPerSale",
            Value = 3.5m,
            MaxAmount = 50000.0m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_E2E_12_ListCommissionPlans_Returns200()
    {
        // Arrange
        await CreateTestCommissionPlan("COM-LST-001");

        // Act
        var response = await _client.GetAsync("/api/v1/commission-plans");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
        body.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TP_CAT_E2E_13_DeleteCommissionPlan_Returns204()
    {
        // Arrange
        var planId = await CreateTestCommissionPlan("COM-DEL-001");

        // Act
        var response = await _client.DeleteAsync($"/api/v1/commission-plans/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

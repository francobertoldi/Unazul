using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SA.Catalog.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Catalog.Tests.E2E;

public sealed class CoverageEndpointsTests : IClassFixture<CatalogWebAppFactory>
{
    private readonly CatalogWebAppFactory _factory;
    private readonly HttpClient _client;

    private static readonly string[] AllPermissions =
    [
        "p_cat_list", "p_cat_create", "p_cat_edit", "p_cat_delete", "p_cat_detail",
        "p_cat_commissions_list", "p_cat_commissions_create", "p_cat_commissions_edit", "p_cat_commissions_delete"
    ];

    public CoverageEndpointsTests(CatalogWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(AllPermissions);
    }

    private async Task<Guid> CreateTestFamily(string code)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/product-families",
            new { Code = code, Description = "Familia para coberturas" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateTestProduct(Guid familyId, string code)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/products", new
        {
            EntityId = CatalogWebAppFactory.TestEntityId,
            FamilyId = familyId,
            Name = "Seguro para coberturas",
            Code = code,
            Description = "Producto seguro test",
            Status = "Active",
            ValidFrom = "2025-01-01"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateTestInsurancePlan(Guid productId, string code)
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/plans", new
        {
            Name = "Plan seguro basico",
            Code = code,
            Price = 2000.00m,
            Currency = "ARS",
            InsuranceAttributes = new
            {
                Premium = 1500.00m,
                SumInsured = 100000.00m,
                GracePeriodDays = 30,
                CoverageType = "Life"
            }
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task TP_CAT_E2E_14_AddCoverage_Returns201()
    {
        // Arrange
        var familyId = await CreateTestFamily("SEG-COV01");
        var productId = await CreateTestProduct(familyId, "SEG-COV-001");
        var planId = await CreateTestInsurancePlan(productId, "PLAN-COV-001");

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/products/{productId}/plans/{planId}/coverages",
            new
            {
                Name = "Cobertura por fallecimiento",
                CoverageType = "Death",
                SumInsured = 500000.0m,
                Premium = 250.0m,
                GracePeriodDays = 30
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_E2E_15_DeleteCoverage_Returns204()
    {
        // Arrange
        var familyId = await CreateTestFamily("SEG-CVD01");
        var productId = await CreateTestProduct(familyId, "SEG-CVD-001");
        var planId = await CreateTestInsurancePlan(productId, "PLAN-CVD-001");

        var addResponse = await _client.PostAsJsonAsync(
            $"/api/v1/products/{productId}/plans/{planId}/coverages",
            new
            {
                Name = "Cobertura para eliminar",
                CoverageType = "Disability",
                SumInsured = 200000.0m,
                Premium = 100.0m,
                GracePeriodDays = 15
            });
        addResponse.EnsureSuccessStatusCode();
        var addBody = await addResponse.Content.ReadFromJsonAsync<JsonElement>();
        var coverageId = addBody.GetProperty("id").GetGuid();

        // Act
        var response = await _client.DeleteAsync(
            $"/api/v1/products/{productId}/plans/{planId}/coverages/{coverageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

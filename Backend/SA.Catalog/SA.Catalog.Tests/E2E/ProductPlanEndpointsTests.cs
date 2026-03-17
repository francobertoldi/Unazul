using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SA.Catalog.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Catalog.Tests.E2E;

public sealed class ProductPlanEndpointsTests : IClassFixture<CatalogWebAppFactory>
{
    private readonly CatalogWebAppFactory _factory;
    private readonly HttpClient _client;

    private static readonly string[] AllPermissions =
    [
        "p_cat_list", "p_cat_create", "p_cat_edit", "p_cat_delete", "p_cat_detail",
        "p_cat_commissions_list", "p_cat_commissions_create", "p_cat_commissions_edit", "p_cat_commissions_delete"
    ];

    public ProductPlanEndpointsTests(CatalogWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(AllPermissions);
    }

    private async Task<Guid> CreateTestFamily(string code)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/product-families",
            new { Code = code, Description = "Familia para planes" });
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
            Name = "Producto para planes",
            Code = code,
            Description = "Producto test",
            Status = "Active",
            ValidFrom = "2025-01-01"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateTestPlan(Guid productId, string code)
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/plans", new
        {
            Name = "Plan basico",
            Code = code,
            Price = 1000.00m,
            Currency = "ARS",
            Installments = 12,
            LoanAttributes = new
            {
                AmortizationType = "French",
                AnnualEffectiveRate = 55.0m,
                CftRate = 65.0m,
                AdminFees = 500.0m
            }
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task TP_CAT_E2E_08_CreatePlan_Returns201()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-PL01");
        var productId = await CreateTestProduct(familyId, "PREST-PL-001");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/plans", new
        {
            Name = "Plan Premium",
            Code = "PLAN-E2E-001",
            Price = 5000.00m,
            Currency = "ARS",
            Installments = 24,
            LoanAttributes = new
            {
                AmortizationType = "French",
                AnnualEffectiveRate = 45.0m,
                CftRate = 55.0m,
                AdminFees = 300.0m
            }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_E2E_09_UpdatePlan_Returns200()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-PU01");
        var productId = await CreateTestProduct(familyId, "PREST-PU-001");
        var planId = await CreateTestPlan(productId, "PLAN-UPD-001");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{productId}/plans/{planId}", new
        {
            Name = "Plan Actualizado",
            Code = "PLAN-UPD-001",
            Price = 7500.00m,
            Currency = "ARS",
            Installments = 36,
            LoanAttributes = new
            {
                AmortizationType = "French",
                AnnualEffectiveRate = 40.0m,
                CftRate = 50.0m,
                AdminFees = 200.0m
            }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().Be(planId);
    }

    [Fact]
    public async Task TP_CAT_E2E_10_DeletePlan_Returns204()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-PD01");
        var productId = await CreateTestProduct(familyId, "PREST-PD-001");
        var planId = await CreateTestPlan(productId, "PLAN-DEL-001");

        // Act
        var response = await _client.DeleteAsync($"/api/v1/products/{productId}/plans/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

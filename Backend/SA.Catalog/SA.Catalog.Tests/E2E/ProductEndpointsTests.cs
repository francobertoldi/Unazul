using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SA.Catalog.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Catalog.Tests.E2E;

public sealed class ProductEndpointsTests : IClassFixture<CatalogWebAppFactory>
{
    private readonly CatalogWebAppFactory _factory;
    private readonly HttpClient _client;

    private static readonly string[] AllPermissions =
    [
        "p_cat_list", "p_cat_create", "p_cat_edit", "p_cat_delete", "p_cat_detail",
        "p_cat_commissions_list", "p_cat_commissions_create", "p_cat_commissions_edit", "p_cat_commissions_delete"
    ];

    public ProductEndpointsTests(CatalogWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(AllPermissions);
    }

    private async Task<Guid> CreateTestFamily(string code = "PREST-PROD", string description = "Familia para productos")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/product-families",
            new { Code = code, Description = description });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateTestProduct(Guid familyId, string code = "PREST-PROD-001", string name = "Prestamo Test")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/products", new
        {
            EntityId = CatalogWebAppFactory.TestEntityId,
            FamilyId = familyId,
            Name = name,
            Code = code,
            Description = "Producto de prueba E2E",
            Status = "Active",
            ValidFrom = "2025-01-01"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task TP_CAT_E2E_04_CreateProduct_Returns201()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-CP01", "Familia crear producto");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", new
        {
            EntityId = CatalogWebAppFactory.TestEntityId,
            FamilyId = familyId,
            Name = "Prestamo Personal E2E",
            Code = "PREST-E2E-001",
            Description = "Test product",
            Status = "Active",
            ValidFrom = "2025-01-01"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_E2E_05_ListProducts_Returns200()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-LP01", "Familia listar productos");
        await CreateTestProduct(familyId, "PREST-LP-001", "Producto listar");

        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TP_CAT_E2E_06_GetProductDetail_Returns200()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-DT01", "Familia detalle producto");
        var productId = await CreateTestProduct(familyId, "PREST-DT-001", "Producto detalle");

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().Be(productId);
    }

    [Fact]
    public async Task TP_CAT_E2E_07_DeprecateProduct_Returns200()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-DP01", "Familia deprecar producto");
        var productId = await CreateTestProduct(familyId, "PREST-DP-001", "Producto deprecar");

        // Act
        var response = await _client.PutAsync($"/api/v1/products/{productId}/deprecate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().Be(productId);
    }
}

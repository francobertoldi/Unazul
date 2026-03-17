using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SA.Catalog.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Catalog.Tests.E2E;

public sealed class ProductFamilyEndpointsTests : IClassFixture<CatalogWebAppFactory>
{
    private readonly CatalogWebAppFactory _factory;
    private readonly HttpClient _client;

    private static readonly string[] AllPermissions =
    [
        "p_cat_list", "p_cat_create", "p_cat_edit", "p_cat_delete", "p_cat_detail",
        "p_cat_commissions_list", "p_cat_commissions_create", "p_cat_commissions_edit", "p_cat_commissions_delete"
    ];

    public ProductFamilyEndpointsTests(CatalogWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(AllPermissions);
    }

    private async Task<Guid> CreateTestFamily(string code = "PREST-001", string description = "Prestamos personales")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/product-families",
            new { Code = code, Description = description });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task TP_CAT_E2E_01_CreateFamily_WithValidPrefix_Returns201()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/product-families",
            new { Code = "PREST-FAM01", Description = "Familia de prestamos E2E" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_E2E_02_ListFamilies_Returns200_WithItems()
    {
        // Arrange
        await CreateTestFamily("PREST-LIST1", "Familia para listar");

        // Act
        var response = await _client.GetAsync("/api/v1/product-families");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
        body.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TP_CAT_E2E_03_DeleteFamily_Returns204()
    {
        // Arrange
        var familyId = await CreateTestFamily("SEG-DEL01", "Familia para eliminar");

        // Act
        var response = await _client.DeleteAsync($"/api/v1/product-families/{familyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

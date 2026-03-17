using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SA.Catalog.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Catalog.Tests.E2E;

public sealed class RequirementEndpointsTests : IClassFixture<CatalogWebAppFactory>
{
    private readonly CatalogWebAppFactory _factory;
    private readonly HttpClient _client;

    private static readonly string[] AllPermissions =
    [
        "p_cat_list", "p_cat_create", "p_cat_edit", "p_cat_delete", "p_cat_detail",
        "p_cat_commissions_list", "p_cat_commissions_create", "p_cat_commissions_edit", "p_cat_commissions_delete"
    ];

    public RequirementEndpointsTests(CatalogWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(AllPermissions);
    }

    private async Task<Guid> CreateTestFamily(string code)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/product-families",
            new { Code = code, Description = "Familia para requisitos" });
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
            Name = "Producto para requisitos",
            Code = code,
            Description = "Producto test requisitos",
            Status = "Active",
            ValidFrom = "2025-01-01"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task TP_CAT_E2E_16_CreateRequirement_Returns201()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-RQ01");
        var productId = await CreateTestProduct(familyId, "PREST-RQ-001");

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/products/{productId}/requirements",
            new
            {
                Name = "DNI del titular",
                Type = "document",
                IsMandatory = true,
                Description = "Documento nacional de identidad"
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_E2E_17_DeleteRequirement_Returns204()
    {
        // Arrange
        var familyId = await CreateTestFamily("PREST-RD01");
        var productId = await CreateTestProduct(familyId, "PREST-RD-001");

        var addResponse = await _client.PostAsJsonAsync(
            $"/api/v1/products/{productId}/requirements",
            new
            {
                Name = "Recibo de sueldo",
                Type = "document",
                IsMandatory = false,
                Description = "Ultimo recibo de sueldo"
            });
        addResponse.EnsureSuccessStatusCode();
        var addBody = await addResponse.Content.ReadFromJsonAsync<JsonElement>();
        var requirementId = addBody.GetProperty("id").GetGuid();

        // Act
        var response = await _client.DeleteAsync(
            $"/api/v1/products/{productId}/requirements/{requirementId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

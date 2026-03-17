using FluentAssertions;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Create_SetsAllPropertiesWithVersionOne()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var product = Product.Create(
            tenantId, entityId, familyId,
            "Test Product", "PROD001", "A test product",
            ProductStatus.Active, new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31),
            userId);

        // Assert
        product.Id.Should().NotBeEmpty();
        product.TenantId.Should().Be(tenantId);
        product.EntityId.Should().Be(entityId);
        product.FamilyId.Should().Be(familyId);
        product.Name.Should().Be("Test Product");
        product.Code.Should().Be("PROD001");
        product.Description.Should().Be("A test product");
        product.Status.Should().Be(ProductStatus.Active);
        product.ValidFrom.Should().Be(new DateOnly(2025, 1, 1));
        product.ValidTo.Should().Be(new DateOnly(2025, 12, 31));
        product.Version.Should().Be(1);
        product.CreatedBy.Should().Be(userId);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Update_IncrementsVersion()
    {
        // Arrange
        var product = CreateProduct();
        var originalVersion = product.Version;

        // Act
        product.Update("Updated", "PROD002", "Updated desc",
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());

        // Assert
        product.Version.Should().Be(originalVersion + 1);
        product.Name.Should().Be("Updated");
    }

    [Fact]
    public void Deprecate_SetsStatusToDeprecated()
    {
        // Arrange
        var product = CreateProduct();
        var userId = Guid.NewGuid();

        // Act
        product.Deprecate(userId);

        // Assert
        product.Status.Should().Be(ProductStatus.Deprecated);
        product.UpdatedBy.Should().Be(userId);
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ValidToAndDescriptionCanBeNull()
    {
        // Act
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "CODE", null,
            ProductStatus.Draft, new DateOnly(2025, 1, 1), null,
            Guid.NewGuid());

        // Assert
        product.Description.Should().BeNull();
        product.ValidTo.Should().BeNull();
    }

    private static Product CreateProduct()
    {
        return Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test Product", "PROD001", "Test",
            ProductStatus.Active, new DateOnly(2025, 1, 1), null,
            Guid.NewGuid());
    }
}

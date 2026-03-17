using FluentAssertions;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Domain;

public sealed class ProductFamilyTests
{
    [Fact]
    public void Create_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var code = "PREST001";
        var description = "Personal Loans";
        var userId = Guid.NewGuid();

        // Act
        var family = ProductFamily.Create(tenantId, code, description, userId);

        // Assert
        family.Id.Should().NotBeEmpty();
        family.TenantId.Should().Be(tenantId);
        family.Code.Should().Be(code);
        family.Description.Should().Be(description);
        family.CreatedBy.Should().Be(userId);
        family.UpdatedBy.Should().Be(userId);
        family.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        family.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Update_ChangesDescriptionAndUpdatedAt()
    {
        // Arrange
        var family = ProductFamily.Create(Guid.NewGuid(), "SEG001", "Seguros", Guid.NewGuid());
        var originalUpdatedAt = family.UpdatedAt;
        var newUserId = Guid.NewGuid();

        // Act
        family.Update("Seguros Actualizado", newUserId);

        // Assert
        family.Description.Should().Be("Seguros Actualizado");
        family.UpdatedBy.Should().Be(newUserId);
        family.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void Create_CodeAndTenantIdAreSetCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var code = "CTA002";

        // Act
        var family = ProductFamily.Create(tenantId, code, "Cuentas", Guid.NewGuid());

        // Assert
        family.Code.Should().Be(code);
        family.TenantId.Should().Be(tenantId);
    }
}

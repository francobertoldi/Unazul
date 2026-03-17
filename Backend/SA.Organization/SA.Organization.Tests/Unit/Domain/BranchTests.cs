using FluentAssertions;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Domain;

public sealed class BranchTests
{
    private static Branch CreateBranch(Guid? entityId = null, Guid? tenantId = null, string code = "BR-001")
    {
        return Branch.Create(
            entityId ?? Guid.NewGuid(),
            tenantId ?? Guid.NewGuid(),
            "Test Branch",
            code,
            "Calle 1", "Buenos Aires", "CABA", "1000", "AR", "+54 11 1234", "branch@test.com",
            true);
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
    }

    [Fact]
    public void TP_ORG_10_01_Create_Returns_Branch_With_All_Fields_And_Valid_Id()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var branch = Branch.Create(
            entityId,
            tenantId,
            "Sucursal Centro",
            "SC-001",
            "Av. Corrientes 1234",
            "Buenos Aires",
            "CABA",
            "1000",
            "AR",
            "+54 11 5555-1234",
            "centro@test.com",
            true);

        var after = DateTime.UtcNow;

        // Assert
        branch.Id.Should().NotBeEmpty();
        branch.EntityId.Should().Be(entityId);
        branch.TenantId.Should().Be(tenantId);
        branch.Name.Should().Be("Sucursal Centro");
        branch.Code.Should().Be("SC-001");
        branch.Address.Should().Be("Av. Corrientes 1234");
        branch.City.Should().Be("Buenos Aires");
        branch.Province.Should().Be("CABA");
        branch.ZipCode.Should().Be("1000");
        branch.Country.Should().Be("AR");
        branch.Phone.Should().Be("+54 11 5555-1234");
        branch.Email.Should().Be("centro@test.com");
        branch.IsActive.Should().BeTrue();
        branch.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        branch.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void TP_ORG_10_02_Update_Changes_Fields_And_UpdatedAt()
    {
        // Arrange
        var branch = CreateBranch();
        var createdAt = branch.CreatedAt;
        var originalUpdatedAt = branch.UpdatedAt;

        // Act
        branch.Update(
            "Updated Branch",
            "New Address",
            "Rosario",
            "Santa Fe",
            "2000",
            "AR",
            "+54 341 999-0000",
            "updated@test.com",
            false);

        // Assert
        branch.Name.Should().Be("Updated Branch");
        branch.Address.Should().Be("New Address");
        branch.City.Should().Be("Rosario");
        branch.Province.Should().Be("Santa Fe");
        branch.ZipCode.Should().Be("2000");
        branch.Country.Should().Be("AR");
        branch.Phone.Should().Be("+54 341 999-0000");
        branch.Email.Should().Be("updated@test.com");
        branch.IsActive.Should().BeFalse();
        branch.CreatedAt.Should().Be(createdAt);
        branch.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void TP_ORG_10_16_TenantId_Inherited_From_Entity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entity = CreateEntity(tenantId);

        // Act
        var branch = Branch.Create(
            entity.Id,
            entity.TenantId,
            "Branch With Inherited TenantId",
            "INH-001",
            null, null, null, null, null, null, null);

        // Assert
        branch.TenantId.Should().Be(tenantId);
        branch.TenantId.Should().Be(entity.TenantId);
        branch.EntityId.Should().Be(entity.Id);
    }
}

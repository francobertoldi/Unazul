using FluentAssertions;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Domain;

public sealed class TenantTests
{
    [Fact]
    public void TP_ORG_02_01_Create_Returns_Tenant_With_All_Fields_And_Valid_Id_And_CreatedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var tenant = Tenant.Create(
            "Test Org",
            "20-12345678-1",
            TenantStatus.Active,
            "Calle 123",
            "Buenos Aires",
            "CABA",
            "1000",
            "AR",
            "+54 11 1234-5678",
            "test@org.com",
            "https://logo.png");

        var after = DateTime.UtcNow;

        // Assert
        tenant.Id.Should().NotBeEmpty();
        tenant.Name.Should().Be("Test Org");
        tenant.Identifier.Should().Be("20-12345678-1");
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.Address.Should().Be("Calle 123");
        tenant.City.Should().Be("Buenos Aires");
        tenant.Province.Should().Be("CABA");
        tenant.ZipCode.Should().Be("1000");
        tenant.Country.Should().Be("AR");
        tenant.Phone.Should().Be("+54 11 1234-5678");
        tenant.Email.Should().Be("test@org.com");
        tenant.LogoUrl.Should().Be("https://logo.png");
        tenant.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        tenant.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void TP_ORG_03_01_Update_Changes_Fields_And_UpdatedAt()
    {
        // Arrange
        var tenant = Tenant.Create(
            "Original",
            "20-12345678-1",
            TenantStatus.Active,
            null, null, null, null, null, null, null, null);

        var createdAt = tenant.CreatedAt;
        var originalUpdatedAt = tenant.UpdatedAt;

        // Act
        tenant.Update(
            "Updated Name",
            TenantStatus.Suspended,
            "New Address",
            "New City",
            "New Province",
            "2000",
            "UY",
            "+598 1234",
            "updated@org.com",
            "https://new-logo.png");

        // Assert
        tenant.Name.Should().Be("Updated Name");
        tenant.Status.Should().Be(TenantStatus.Suspended);
        tenant.Address.Should().Be("New Address");
        tenant.City.Should().Be("New City");
        tenant.Province.Should().Be("New Province");
        tenant.ZipCode.Should().Be("2000");
        tenant.Country.Should().Be("UY");
        tenant.Phone.Should().Be("+598 1234");
        tenant.Email.Should().Be("updated@org.com");
        tenant.LogoUrl.Should().Be("https://new-logo.png");
        tenant.CreatedAt.Should().Be(createdAt);
        tenant.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void TP_ORG_02_02_Create_With_Inactive_Status_Works()
    {
        // Arrange & Act
        var tenant = Tenant.Create(
            "Inactive Org",
            "20-99999999-1",
            TenantStatus.Inactive,
            null, null, null, null, null, null, null, null);

        // Assert
        tenant.Status.Should().Be(TenantStatus.Inactive);
        tenant.Id.Should().NotBeEmpty();
        tenant.Name.Should().Be("Inactive Org");
        tenant.Identifier.Should().Be("20-99999999-1");
    }

    [Fact]
    public void TP_ORG_03_03_Create_With_Suspended_Status_Works()
    {
        var tenant = Tenant.Create(
            "Suspended Org", "20-55555555-1", TenantStatus.Suspended,
            null, null, null, null, null, null, null, null);

        tenant.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public void TP_ORG_02_01b_Create_With_Null_Optional_Fields_DefaultsToNull()
    {
        var tenant = Tenant.Create(
            "Minimal Org", "20-11111111-1", TenantStatus.Active,
            null, null, null, null, null, null, null, null);

        tenant.Address.Should().BeNull();
        tenant.City.Should().BeNull();
        tenant.Province.Should().BeNull();
        tenant.ZipCode.Should().BeNull();
        tenant.Country.Should().BeNull();
        tenant.Phone.Should().BeNull();
        tenant.Email.Should().BeNull();
        tenant.LogoUrl.Should().BeNull();
    }

    [Fact]
    public void TP_ORG_03_01b_Update_PreservesIdentifier()
    {
        var tenant = Tenant.Create(
            "Original", "20-12345678-1", TenantStatus.Active,
            null, null, null, null, null, null, null, null);

        tenant.Update("Updated", TenantStatus.Inactive,
            null, null, null, null, null, null, null, null);

        tenant.Identifier.Should().Be("20-12345678-1");
    }
}

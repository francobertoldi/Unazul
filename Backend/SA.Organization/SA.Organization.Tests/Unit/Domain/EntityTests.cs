using FluentAssertions;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Domain;

public sealed class EntityTests
{
    [Fact]
    public void TP_ORG_07_01_Create_WithValidData_ReturnsEntityWithCorrectFields()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "Test Entity";
        var cuit = "20-12345678-1";
        var type = EntityType.Bank;
        var status = EntityStatus.Active;
        var address = "Av. Corrientes 1234";
        var city = "Buenos Aires";
        var province = "CABA";
        var zipCode = "C1000";
        var country = "Argentina";
        var phone = "+54 11 1234-5678";
        var email = "test@entity.com";

        // Act
        var entity = Entity.Create(tenantId, name, cuit, type, status,
            address, city, province, zipCode, country, phone, email);

        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.TenantId.Should().Be(tenantId);
        entity.Name.Should().Be(name);
        entity.Cuit.Should().Be(cuit);
        entity.Type.Should().Be(type);
        entity.Status.Should().Be(status);
        entity.Address.Should().Be(address);
        entity.City.Should().Be(city);
        entity.Province.Should().Be(province);
        entity.ZipCode.Should().Be(zipCode);
        entity.Country.Should().Be(country);
        entity.Phone.Should().Be(phone);
        entity.Email.Should().Be(email);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TP_ORG_07_03_Update_ChangesFieldsAndUpdatedAt()
    {
        // Arrange
        var entity = CreateEntity();
        var originalUpdatedAt = entity.UpdatedAt;

        // Act
        entity.Update("Updated Name", EntityType.Fintech, EntityStatus.Suspended,
            "New Address", "Rosario", "Santa Fe", "S2000", "Argentina",
            "+54 341 555-0000", "updated@entity.com");

        // Assert
        entity.Name.Should().Be("Updated Name");
        entity.Type.Should().Be(EntityType.Fintech);
        entity.Status.Should().Be(EntityStatus.Suspended);
        entity.Address.Should().Be("New Address");
        entity.City.Should().Be("Rosario");
        entity.Province.Should().Be("Santa Fe");
        entity.ZipCode.Should().Be("S2000");
        entity.Country.Should().Be("Argentina");
        entity.Phone.Should().Be("+54 341 555-0000");
        entity.Email.Should().Be("updated@entity.com");
        entity.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void TP_ORG_07_07_Create_WithoutChannels_HasEmptyChannelsCollection()
    {
        // Act
        var entity = CreateEntity();

        // Assert
        entity.Channels.Should().NotBeNull();
        entity.Channels.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ChannelType.Web)]
    [InlineData(ChannelType.Mobile)]
    [InlineData(ChannelType.Api)]
    [InlineData(ChannelType.Presencial)]
    [InlineData(ChannelType.IaAgent)]
    public void EntityChannel_Create_WorksWithAllChannelTypes(ChannelType channelType)
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var channel = EntityChannel.Create(entityId, tenantId, channelType);

        // Assert
        channel.Id.Should().NotBeEmpty();
        channel.EntityId.Should().Be(entityId);
        channel.TenantId.Should().Be(tenantId);
        channel.ChannelType.Should().Be(channelType);
        channel.IsActive.Should().BeTrue();
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
    }
}

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Commands.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Tenants;

public sealed class UpdateTenantCommandHandlerTests
{
    private readonly ITenantRepository _repo = Substitute.For<ITenantRepository>();
    private readonly UpdateTenantCommandHandler _sut;

    public UpdateTenantCommandHandlerTests()
    {
        _sut = new UpdateTenantCommandHandler(_repo);
    }

    private static Tenant CreateTenant(
        string name = "Test Org",
        string identifier = "20-12345678-1") =>
        Tenant.Create(name, identifier, TenantStatus.Active, null, null, null, null, null, null, null, null);

    private static UpdateTenantCommand CreateValidCommand(
        Guid? id = null,
        string name = "Updated Org",
        string status = "Active") =>
        new(id ?? Guid.NewGuid(), name, status, "Calle 2", "Cordoba", "CBA", "5000", "AR", "+54 351 999", "updated@org.com", "https://logo.png");

    [Fact]
    public async Task TP_ORG_03_01_Successful_Update_Returns_TenantDto()
    {
        // Arrange
        var tenant = CreateTenant();
        var command = CreateValidCommand(id: tenant.Id, name: "Updated Org", status: "Suspended");

        _repo.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(tenant.Id);
        result.Name.Should().Be("Updated Org");
        result.Status.Should().Be("Suspended");
        result.Address.Should().Be("Calle 2");
        result.City.Should().Be("Cordoba");
        result.Province.Should().Be("CBA");
        result.Phone.Should().Be("+54 351 999");
        result.Email.Should().Be("updated@org.com");
        _repo.Received(1).Update(tenant);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_03_05_Not_Found_Throws_ORG_TENANT_NOT_FOUND()
    {
        // Arrange
        var command = CreateValidCommand();
        _repo.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_TENANT_NOT_FOUND");
    }

    [Fact]
    public async Task TP_ORG_03_06_Invalid_Status_Throws_ORG_INVALID_STATUS()
    {
        // Arrange
        var tenant = CreateTenant();
        var command = CreateValidCommand(id: tenant.Id, status: "InvalidStatus");

        _repo.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_STATUS");
    }

    [Fact]
    public async Task TP_ORG_03_04_Verify_Update_And_SaveChangesAsync_Called()
    {
        // Arrange
        var tenant = CreateTenant();
        var command = CreateValidCommand(id: tenant.Id);

        _repo.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _repo.Received(1).Update(tenant);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

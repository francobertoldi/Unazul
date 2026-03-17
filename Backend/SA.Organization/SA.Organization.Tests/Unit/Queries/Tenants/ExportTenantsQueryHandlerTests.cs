using FluentAssertions;
using NSubstitute;
using SA.Organization.Application.Queries.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Queries.Tenants;

public sealed class ExportTenantsQueryHandlerTests
{
    private readonly ITenantRepository _repo = Substitute.For<ITenantRepository>();
    private readonly ExportTenantsQueryHandler _sut;

    public ExportTenantsQueryHandlerTests()
    {
        _sut = new ExportTenantsQueryHandler(_repo);
    }

    private static Tenant CreateTenant(
        string name = "Test Org",
        string identifier = "20-12345678-1") =>
        Tenant.Create(name, identifier, TenantStatus.Active, null, "Buenos Aires", "CABA", null, null, "+54 11 1234", "test@org.com", null);

    [Fact]
    public async Task TP_ORG_01_04_Excel_Export_Returns_Non_Empty_Byte_Array()
    {
        // Arrange
        var tenants = new List<Tenant> { CreateTenant("Org A"), CreateTenant("Org B", "20-99999999-1") };
        var query = new ExportTenantsQuery("xlsx", null, null);

        _repo.ListForExportAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(tenants.AsReadOnly());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TP_ORG_01_05_Csv_Export_Returns_Non_Empty_Byte_Array()
    {
        // Arrange
        var tenants = new List<Tenant> { CreateTenant("Org A"), CreateTenant("Org B", "20-99999999-1") };
        var query = new ExportTenantsQuery("csv", null, null);

        _repo.ListForExportAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(tenants.AsReadOnly());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Length.Should().BeGreaterThan(0);
    }
}

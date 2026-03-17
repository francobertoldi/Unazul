using FluentAssertions;
using NSubstitute;
using SA.Organization.Application.Queries.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Queries.Tenants;

public sealed class ListTenantsQueryHandlerTests
{
    private readonly ITenantRepository _repo = Substitute.For<ITenantRepository>();
    private readonly ListTenantsQueryHandler _sut;

    public ListTenantsQueryHandlerTests()
    {
        _sut = new ListTenantsQueryHandler(_repo);
    }

    private static Tenant CreateTenant(
        string name = "Test Org",
        string identifier = "20-12345678-1") =>
        Tenant.Create(name, identifier, TenantStatus.Active, null, null, null, null, null, null, null, null);

    [Fact]
    public async Task TP_ORG_01_01_Returns_Paged_Result_With_Correct_Mapping()
    {
        // Arrange
        var tenant1 = CreateTenant("Org A", "20-11111111-1");
        var tenant2 = CreateTenant("Org B", "20-22222222-1");
        var query = new ListTenantsQuery(1, 10, null, null, null, "asc");

        _repo.ListAsync(0, 10, null, null, null, "asc", Arg.Any<CancellationToken>())
            .Returns((new List<Tenant> { tenant1, tenant2 }.AsReadOnly(), 2));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items[0].Name.Should().Be("Org A");
        result.Items[1].Name.Should().Be("Org B");
    }

    [Fact]
    public async Task TP_ORG_01_02_Search_Parameter_Passed_Through()
    {
        // Arrange
        var tenant = CreateTenant("Searched Org");
        var query = new ListTenantsQuery(1, 10, "Searched", null, null, "asc");

        _repo.ListAsync(0, 10, "Searched", null, null, "asc", Arg.Any<CancellationToken>())
            .Returns((new List<Tenant> { tenant }.AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Searched Org");
        await _repo.Received(1).ListAsync(0, 10, "Searched", null, null, "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_01_03_Status_Filter_Passed_Through()
    {
        // Arrange
        var query = new ListTenantsQuery(1, 10, null, "Active", null, "asc");

        _repo.ListAsync(0, 10, null, "Active", null, "asc", Arg.Any<CancellationToken>())
            .Returns((new List<Tenant>().AsReadOnly(), 0));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _repo.Received(1).ListAsync(0, 10, null, "Active", null, "asc", Arg.Any<CancellationToken>());
        result.Total.Should().Be(0);
    }
}

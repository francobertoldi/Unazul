using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Queries.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Queries.Tenants;

public sealed class GetTenantDetailQueryHandlerTests
{
    private readonly ITenantRepository _repo = Substitute.For<ITenantRepository>();
    private readonly GetTenantDetailQueryHandler _sut;

    public GetTenantDetailQueryHandlerTests()
    {
        _sut = new GetTenantDetailQueryHandler(_repo);
    }

    private static Tenant CreateTenant(
        string name = "Test Org",
        string identifier = "20-12345678-1") =>
        Tenant.Create(name, identifier, TenantStatus.Active,
            "Calle 1", "Buenos Aires", "CABA", "1000", "AR", "+54 11 1234", "test@org.com", "https://logo.png");

    [Fact]
    public async Task TP_ORG_04_01_Returns_Complete_Detail_With_Entity_Count()
    {
        // Arrange
        var tenant = CreateTenant();
        var query = new GetTenantDetailQuery(tenant.Id);

        _repo.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);
        _repo.CountEntitiesAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(5);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Id.Should().Be(tenant.Id);
        result.Name.Should().Be("Test Org");
        result.Identifier.Should().Be("20-12345678-1");
        result.Status.Should().Be("Active");
        result.Address.Should().Be("Calle 1");
        result.City.Should().Be("Buenos Aires");
        result.Province.Should().Be("CABA");
        result.ZipCode.Should().Be("1000");
        result.Country.Should().Be("AR");
        result.Phone.Should().Be("+54 11 1234");
        result.Email.Should().Be("test@org.com");
        result.LogoUrl.Should().Be("https://logo.png");
        result.EntityCount.Should().Be(5);
        result.UserCount.Should().Be(0);
    }

    [Fact]
    public async Task TP_ORG_04_02_Not_Found_Throws_ORG_TENANT_NOT_FOUND()
    {
        // Arrange
        var query = new GetTenantDetailQuery(Guid.NewGuid());

        _repo.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_TENANT_NOT_FOUND");
    }

    [Fact]
    public async Task TP_ORG_04_03_Entity_Count_Of_Zero_Works()
    {
        // Arrange
        var tenant = CreateTenant();
        var query = new GetTenantDetailQuery(tenant.Id);

        _repo.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);
        _repo.CountEntitiesAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.EntityCount.Should().Be(0);
        result.UserCount.Should().Be(0);
        result.Name.Should().Be("Test Org");
    }
}

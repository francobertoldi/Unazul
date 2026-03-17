using FluentAssertions;
using NSubstitute;
using SA.Config.Application.Queries.ExternalServices;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Config.Tests.Unit.Queries.ExternalServices;

public sealed class ListExternalServicesQueryHandlerTests
{
    private readonly IExternalServiceRepository _repo = Substitute.For<IExternalServiceRepository>();
    private readonly ListExternalServicesQueryHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ListExternalServicesQueryHandlerTests()
    {
        _sut = new ListExternalServicesQueryHandler(_repo);
    }

    private static ExternalService CreateService(
        string name = "TestService",
        AuthType authType = AuthType.ApiKey,
        ServiceStatus status = ServiceStatus.Active)
    {
        return ExternalService.Create(
            TenantId, name, "desc", ServiceType.RestApi, "https://api.example.com",
            status, 30000, 3, authType, Guid.NewGuid());
    }

    [Fact]
    public async Task TP_CFG_08_01_Returns_Services_Without_Credentials()
    {
        // Arrange
        var service = CreateService();
        service.AuthConfigs.Add(ServiceAuthConfig.Create(service.Id, "api_key", "encrypted_secret"));

        _repo.GetAllByTenantAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExternalService> { service });

        // Act
        var result = await _sut.Handle(new ListExternalServicesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Name.Should().Be("TestService");
        dto.AuthType.Should().Be(AuthType.ApiKey);
        // DTO should not contain credential fields - it only has the shape of ExternalServiceDto
        dto.Id.Should().Be(service.Id);
    }

    [Fact]
    public async Task TP_CFG_08_02_Returns_Services_With_TestInfo()
    {
        // Arrange
        var service = CreateService();
        service.RecordTestResult(true, DateTime.UtcNow);

        _repo.GetAllByTenantAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExternalService> { service });

        // Act
        var result = await _sut.Handle(new ListExternalServicesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].LastTestedAt.Should().NotBeNull();
        result[0].LastTestSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TP_CFG_08_03_Returns_Empty_List()
    {
        // Arrange
        _repo.GetAllByTenantAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExternalService>());

        // Act
        var result = await _sut.Handle(new ListExternalServicesQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

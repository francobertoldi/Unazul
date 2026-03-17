using FluentAssertions;
using SA.Config.DataAccess.EntityFramework.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Tests.Integration.Fixtures;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Config.Tests.Integration.Repositories;

public sealed class ExternalServiceRepositoryIntegrationTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public ExternalServiceRepositoryIntegrationTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Creates_Service_With_Name_Uniqueness()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new ExternalServiceRepository(db);

        var service = ExternalService.Create(
            TenantId, "UniqueService", "A service", ServiceType.RestApi,
            "https://api.example.com", ServiceStatus.Active, 5000, 3, AuthType.None, UserId);

        // Act
        await repo.AddAsync(service);
        await repo.SaveChangesAsync();

        // Assert
        var exists = await repo.ExistsByNameAsync(TenantId, "UniqueService");
        exists.Should().BeTrue();

        var notExists = await repo.ExistsByNameAsync(TenantId, "OtherService");
        notExists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdWithAuthConfigs_Loads_Configs()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new ExternalServiceRepository(db);

        var service = ExternalService.Create(
            TenantId, "AuthService", null, ServiceType.RestApi,
            "https://auth.api.com", ServiceStatus.Active, 5000, 3, AuthType.ApiKey, UserId);
        await repo.AddAsync(service);
        await repo.SaveChangesAsync();

        var configs = new[]
        {
            ServiceAuthConfig.Create(service.Id, "header_name", "encrypted_X-Api-Key"),
            ServiceAuthConfig.Create(service.Id, "api_key", "encrypted_secret"),
        };
        await repo.ReplaceAuthConfigsAsync(service.Id, configs);
        await repo.SaveChangesAsync();

        // Act
        var loaded = await repo.GetByIdWithAuthConfigsAsync(service.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.AuthConfigs.Should().HaveCount(2);
        loaded.AuthConfigs.Should().Contain(c => c.Key == "header_name");
        loaded.AuthConfigs.Should().Contain(c => c.Key == "api_key");
    }

    [Fact]
    public async Task ExistsByName_With_ExcludeId()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new ExternalServiceRepository(db);

        var service = ExternalService.Create(
            TenantId, "ExcludeTest", null, ServiceType.RestApi,
            "https://api.com", ServiceStatus.Active, 5000, 3, AuthType.None, UserId);
        await repo.AddAsync(service);
        await repo.SaveChangesAsync();

        // Act - check with excludeId (should return false when excluding self)
        var existsExcludingSelf = await repo.ExistsByNameAsync(TenantId, "ExcludeTest", service.Id);
        var existsWithoutExclude = await repo.ExistsByNameAsync(TenantId, "ExcludeTest");

        // Assert
        existsExcludingSelf.Should().BeFalse();
        existsWithoutExclude.Should().BeTrue();
    }
}

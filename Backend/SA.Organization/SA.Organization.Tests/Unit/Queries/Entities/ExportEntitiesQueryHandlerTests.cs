using FluentAssertions;
using NSubstitute;
using SA.Organization.Application.Queries.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Queries.Entities;

public sealed class ExportEntitiesQueryHandlerTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly ExportEntitiesQueryHandler _sut;

    public ExportEntitiesQueryHandlerTests()
    {
        _sut = new ExportEntitiesQueryHandler(_entityRepository);
    }

    [Fact]
    public async Task TP_ORG_06_05_Export_Excel_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var entity = CreateEntity();
        var query = new ExportEntitiesQuery("xlsx", null, null, null);

        _entityRepository.ListForExportAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Entity> { entity } as IReadOnlyList<Entity>);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_ORG_06_06_Export_Csv_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var entity = CreateEntity();
        var query = new ExportEntitiesQuery("csv", null, null, null);

        _entityRepository.ListForExportAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Entity> { entity } as IReadOnlyList<Entity>);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, "Buenos Aires", "CABA", null, null, null, "test@test.com");
    }
}

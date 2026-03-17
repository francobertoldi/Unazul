using FluentAssertions;
using NSubstitute;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.Application.Queries.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Queries.Entities;

public sealed class ListEntitiesQueryHandlerTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly ListEntitiesQueryHandler _sut;

    public ListEntitiesQueryHandlerTests()
    {
        _sut = new ListEntitiesQueryHandler(_entityRepository);
    }

    [Fact]
    public async Task TP_ORG_06_01_List_ReturnsPagedResultWithCorrectMapping()
    {
        // Arrange
        var entity = CreateEntity();
        var query = new ListEntitiesQuery(1, 10, null, null, null, null, "asc");

        _entityRepository.ListAsync(0, 10, null, null, null, null, "asc", Arg.Any<CancellationToken>())
            .Returns((new List<Entity> { entity } as IReadOnlyList<Entity>, 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

        var item = result.Items[0];
        item.Id.Should().Be(entity.Id);
        item.Name.Should().Be(entity.Name);
        item.Cuit.Should().Be(entity.Cuit);
        item.Type.Should().Be("Bank");
        item.Status.Should().Be("Active");
    }

    [Fact]
    public async Task TP_ORG_06_02_List_SearchPassedThrough()
    {
        // Arrange
        var query = new ListEntitiesQuery(1, 10, "test search", null, null, null, "asc");

        _entityRepository.ListAsync(0, 10, "test search", null, null, null, "asc", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Entity>() as IReadOnlyList<Entity>, 0));

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _entityRepository.Received(1).ListAsync(
            0, 10, "test search", null, null, null, "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_06_03_List_TypeFilterPassedThrough()
    {
        // Arrange
        var query = new ListEntitiesQuery(1, 10, null, null, "Bank", null, "asc");

        _entityRepository.ListAsync(0, 10, null, null, "Bank", null, "asc", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Entity>() as IReadOnlyList<Entity>, 0));

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _entityRepository.Received(1).ListAsync(
            0, 10, null, null, "Bank", null, "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_06_04_List_StatusFilterPassedThrough()
    {
        // Arrange
        var query = new ListEntitiesQuery(1, 10, null, "Active", null, null, "asc");

        _entityRepository.ListAsync(0, 10, null, "Active", null, null, "asc", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Entity>() as IReadOnlyList<Entity>, 0));

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _entityRepository.Received(1).ListAsync(
            0, 10, null, "Active", null, null, "asc", Arg.Any<CancellationToken>());
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, "Buenos Aires", "CABA", null, null, null, null);
    }
}

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.Application.Queries.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Queries.Entities;

public sealed class GetEntityDetailQueryHandlerTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly GetEntityDetailQueryHandler _sut;

    public GetEntityDetailQueryHandlerTests()
    {
        _sut = new GetEntityDetailQueryHandler(_entityRepository);
    }

    [Fact]
    public async Task TP_ORG_08_01_GetDetail_ReturnsFullDetailWithChannelsAndBranches()
    {
        // Arrange
        var entity = CreateEntity();
        // Entity.Create returns empty collections; simulate populated entity
        entity.Channels.Add(EntityChannel.Create(entity.Id, entity.TenantId, ChannelType.Web));
        entity.Channels.Add(EntityChannel.Create(entity.Id, entity.TenantId, ChannelType.Mobile));
        entity.Branches.Add(Branch.Create(entity.Id, entity.TenantId, "Branch 1", "BR001",
            null, null, null, null, null, null, null));

        var query = new GetEntityDetailQuery(entity.Id);

        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<EntityDetailDto>();
        result.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
        result.Channels.Should().HaveCount(2);
        result.Branches.Should().HaveCount(1);
        result.Branches[0].Name.Should().Be("Branch 1");
    }

    [Fact]
    public async Task TP_ORG_08_05_GetDetail_EntityWithEmptyBranches_ReturnsEmptyList()
    {
        // Arrange
        var entity = CreateEntity();
        var query = new GetEntityDetailQuery(entity.Id);

        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Branches.Should().NotBeNull();
        result.Branches.Should().BeEmpty();
    }

    [Fact]
    public async Task TP_ORG_08_06_GetDetail_NotFound_ThrowsOrgEntityNotFound()
    {
        // Arrange
        var query = new GetEntityDetailQuery(Guid.NewGuid());

        _entityRepository.GetByIdWithDetailsAsync(query.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_ENTITY_NOT_FOUND");
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            "Address", "Buenos Aires", "CABA", "C1000", "Argentina",
            "+54 11 1234", "test@test.com");
    }
}

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Commands.Entities;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Entities;

public sealed class UpdateEntityCommandHandlerTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly IEntityChannelRepository _channelRepository = Substitute.For<IEntityChannelRepository>();
    private readonly UpdateEntityCommandHandler _sut;

    public UpdateEntityCommandHandlerTests()
    {
        _sut = new UpdateEntityCommandHandler(_entityRepository, _channelRepository);
    }

    [Fact]
    public async Task TP_ORG_07_03_Update_WithValidData_ReturnsEntityDetailDto()
    {
        // Arrange
        var entity = CreateEntity();
        var command = new UpdateEntityCommand(
            entity.Id, "Updated Name", "Fintech", "Inactive",
            "New Address", "Rosario", "Santa Fe", "S2000", "Argentina",
            "+54 341 555-0000", "updated@test.com", null);

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<EntityDetailDto>();
        result.Name.Should().Be("Updated Name");
        _entityRepository.Received(1).Update(entity);
        await _entityRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_04_Update_ChannelDiff_RemovesOldAndAddsNew()
    {
        // Arrange
        var entity = CreateEntity();
        var existingChannel = EntityChannel.Create(entity.Id, entity.TenantId, ChannelType.Web);

        var command = new UpdateEntityCommand(
            entity.Id, "Test Entity", "Bank", "Active",
            null, null, null, null, null, null, null,
            new[] { "Mobile", "Api" });

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        _channelRepository.GetByEntityIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(new List<EntityChannel> { existingChannel });

        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _channelRepository.Received(1).RemoveRange(
            Arg.Is<List<EntityChannel>>(list => list.Count == 1));
        await _channelRepository.Received(1).AddRangeAsync(
            Arg.Is<List<EntityChannel>>(list => list.Count == 2),
            Arg.Any<CancellationToken>());
        await _channelRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_14_Update_EntityNotFound_ThrowsOrgEntityNotFound()
    {
        // Arrange
        var command = new UpdateEntityCommand(
            Guid.NewGuid(), "Test", "Bank", "Active",
            null, null, null, null, null, null, null, null);

        _entityRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_ENTITY_NOT_FOUND");
    }

    [Fact]
    public async Task Update_InvalidType_ThrowsOrgInvalidEntityType()
    {
        // Arrange
        var entity = CreateEntity();
        var command = new UpdateEntityCommand(
            entity.Id, "Test", "InvalidType", "Active",
            null, null, null, null, null, null, null, null);

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_ENTITY_TYPE");
    }

    [Fact]
    public async Task Update_InvalidStatus_ThrowsOrgInvalidStatus()
    {
        // Arrange
        var entity = CreateEntity();
        var command = new UpdateEntityCommand(
            entity.Id, "Test", "Bank", "InvalidStatus",
            null, null, null, null, null, null, null, null);

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_STATUS");
    }

    [Fact]
    public async Task Update_InvalidChannelType_ThrowsOrgInvalidChannelType()
    {
        // Arrange
        var entity = CreateEntity();
        var command = new UpdateEntityCommand(
            entity.Id, "Test", "Bank", "Active",
            null, null, null, null, null, null, null,
            new[] { "InvalidChannel" });

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        _channelRepository.GetByEntityIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(new List<EntityChannel>());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_CHANNEL_TYPE");
    }

    [Fact]
    public async Task Update_NullChannels_ChannelDiffSkipped()
    {
        // Arrange
        var entity = CreateEntity();
        var command = new UpdateEntityCommand(
            entity.Id, "Updated Name", "Bank", "Active",
            null, null, null, null, null, null, null, null);

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _channelRepository.DidNotReceive().GetByEntityIdAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _channelRepository.DidNotReceive().RemoveRange(Arg.Any<IEnumerable<EntityChannel>>());
        await _channelRepository.DidNotReceive().AddRangeAsync(
            Arg.Any<IEnumerable<EntityChannel>>(), Arg.Any<CancellationToken>());
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
    }
}

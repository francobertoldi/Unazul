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

public sealed class UpdateEntityChannelDiffTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly IEntityChannelRepository _channelRepository = Substitute.For<IEntityChannelRepository>();
    private readonly UpdateEntityCommandHandler _sut;

    public UpdateEntityChannelDiffTests()
    {
        _sut = new UpdateEntityCommandHandler(_entityRepository, _channelRepository);
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task TP_ORG_07_04b_ChannelDiff_RemovesChannels_WhenNewListEmpty()
    {
        // Arrange
        var entity = CreateEntity();
        var existingChannel = EntityChannel.Create(entity.Id, entity.TenantId, ChannelType.Web);

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _channelRepository.GetByEntityIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(new List<EntityChannel> { existingChannel });
        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var command = new UpdateEntityCommand(entity.Id, "Updated", "Bank", "Active",
            null, null, null, null, null, null, null, Array.Empty<string>());

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _channelRepository.Received(1).RemoveRange(Arg.Is<List<EntityChannel>>(x => x.Count == 1));
    }

    [Fact]
    public async Task TP_ORG_07_04c_ChannelDiff_AddsChannels_WhenExistingEmpty()
    {
        var entity = CreateEntity();
        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _channelRepository.GetByEntityIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(new List<EntityChannel>());
        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var command = new UpdateEntityCommand(entity.Id, "Updated", "Bank", "Active",
            null, null, null, null, null, null, null, new[] { "Web", "Mobile" });

        await _sut.Handle(command, CancellationToken.None);

        await _channelRepository.Received(1).AddRangeAsync(
            Arg.Is<List<EntityChannel>>(x => x.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_04d_ChannelDiff_NoChanges_WhenSameChannels()
    {
        var entity = CreateEntity();
        var existingChannel = EntityChannel.Create(entity.Id, entity.TenantId, ChannelType.Web);

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _channelRepository.GetByEntityIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(new List<EntityChannel> { existingChannel });
        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var command = new UpdateEntityCommand(entity.Id, "Updated", "Bank", "Active",
            null, null, null, null, null, null, null, new[] { "Web" });

        await _sut.Handle(command, CancellationToken.None);

        _channelRepository.DidNotReceive().RemoveRange(Arg.Any<IEnumerable<EntityChannel>>());
        await _channelRepository.DidNotReceive().AddRangeAsync(
            Arg.Any<IEnumerable<EntityChannel>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_04e_NullChannels_SkipsChannelDiff()
    {
        var entity = CreateEntity();
        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var command = new UpdateEntityCommand(entity.Id, "Updated", "Bank", "Active",
            null, null, null, null, null, null, null, null);

        await _sut.Handle(command, CancellationToken.None);

        await _channelRepository.DidNotReceive().GetByEntityIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_05_UpdateEntity_SavesChanges()
    {
        var entity = CreateEntity();
        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var command = new UpdateEntityCommand(entity.Id, "Updated Name", "Insurance", "Inactive",
            "New Address", null, null, null, null, null, null, null);

        var result = await _sut.Handle(command, CancellationToken.None);

        _entityRepository.Received(1).Update(entity);
        await _entityRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_06b_ChannelDiff_DuplicatesInNewList_AreDeduped()
    {
        var entity = CreateEntity();
        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _channelRepository.GetByEntityIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(new List<EntityChannel>());
        _entityRepository.GetByIdWithDetailsAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var command = new UpdateEntityCommand(entity.Id, "Updated", "Bank", "Active",
            null, null, null, null, null, null, null, new[] { "Web", "web", "WEB", "Mobile" });

        await _sut.Handle(command, CancellationToken.None);

        await _channelRepository.Received(1).AddRangeAsync(
            Arg.Is<List<EntityChannel>>(x => x.Count == 2),
            Arg.Any<CancellationToken>());
    }
}

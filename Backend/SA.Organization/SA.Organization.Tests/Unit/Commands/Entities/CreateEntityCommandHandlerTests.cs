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

public sealed class CreateEntityCommandHandlerTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly IEntityChannelRepository _channelRepository = Substitute.For<IEntityChannelRepository>();
    private readonly CreateEntityCommandHandler _sut;

    public CreateEntityCommandHandlerTests()
    {
        _sut = new CreateEntityCommandHandler(_entityRepository, _channelRepository);
    }

    [Fact]
    public async Task TP_ORG_07_01_Create_WithValidData_ReturnsEntityDetailDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEntityCommand(
            tenantId, "Test Entity", "20-12345678-1", "Bank", "Active",
            "Address", "City", "Province", "1000", "Argentina",
            "+54 11 1234", "test@test.com", null);

        _entityRepository.ExistsByCuitAsync(tenantId, command.Cuit, Arg.Any<CancellationToken>())
            .Returns(false);

        _entityRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci => CreateEntity(tenantId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<EntityDetailDto>();
        result.Name.Should().Be("Test Entity");
        result.TenantId.Should().Be(tenantId);
        await _entityRepository.Received(1).AddAsync(Arg.Any<Entity>(), Arg.Any<CancellationToken>());
        await _entityRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_02_Create_WithChannels_CallsChannelRepositoryAddRange()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEntityCommand(
            tenantId, "Test Entity", "20-12345678-1", "Bank", "Active",
            null, null, null, null, null, null, null,
            new[] { "Web", "Mobile" });

        _entityRepository.ExistsByCuitAsync(tenantId, command.Cuit, Arg.Any<CancellationToken>())
            .Returns(false);

        _entityRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci => CreateEntity(tenantId));

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _channelRepository.Received(1).AddRangeAsync(
            Arg.Is<List<EntityChannel>>(list => list.Count == 2),
            Arg.Any<CancellationToken>());
        await _channelRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_06_Create_DuplicateChannels_AreDeduped()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEntityCommand(
            tenantId, "Test Entity", "20-12345678-1", "Bank", "Active",
            null, null, null, null, null, null, null,
            new[] { "Web", "web", "Web" });

        _entityRepository.ExistsByCuitAsync(tenantId, command.Cuit, Arg.Any<CancellationToken>())
            .Returns(false);

        _entityRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci => CreateEntity(tenantId));

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _channelRepository.Received(1).AddRangeAsync(
            Arg.Is<List<EntityChannel>>(list => list.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_07_Create_NullChannels_ChannelRepositoryNotCalled()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEntityCommand(
            tenantId, "Test Entity", "20-12345678-1", "Bank", "Active",
            null, null, null, null, null, null, null, null);

        _entityRepository.ExistsByCuitAsync(tenantId, command.Cuit, Arg.Any<CancellationToken>())
            .Returns(false);

        _entityRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci => CreateEntity(tenantId));

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _channelRepository.DidNotReceive().AddRangeAsync(
            Arg.Any<IEnumerable<EntityChannel>>(), Arg.Any<CancellationToken>());
        await _channelRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_08_Create_DuplicateCuit_ThrowsOrgDuplicateCuit()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEntityCommand(
            tenantId, "Test Entity", "20-12345678-1", "Bank", "Active",
            null, null, null, null, null, null, null, null);

        _entityRepository.ExistsByCuitAsync(tenantId, command.Cuit, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_DUPLICATE_CUIT");
    }

    [Fact]
    public async Task TP_ORG_07_09_Create_InvalidType_ThrowsOrgInvalidEntityType()
    {
        // Arrange
        var command = new CreateEntityCommand(
            Guid.NewGuid(), "Test Entity", "20-12345678-1", "InvalidType", "Active",
            null, null, null, null, null, null, null, null);

        _entityRepository.ExistsByCuitAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_ENTITY_TYPE");
    }

    [Fact]
    public async Task TP_ORG_07_13_Create_InvalidChannel_ThrowsOrgInvalidChannelType()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEntityCommand(
            tenantId, "Test Entity", "20-12345678-1", "Bank", "Active",
            null, null, null, null, null, null, null,
            new[] { "InvalidChannel" });

        _entityRepository.ExistsByCuitAsync(tenantId, command.Cuit, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_CHANNEL_TYPE");
    }

    [Fact]
    public async Task Create_InvalidCuitFormat_ThrowsOrgInvalidCuitFormat()
    {
        // Arrange
        var command = new CreateEntityCommand(
            Guid.NewGuid(), "Test Entity", "12345678", "Bank", "Active",
            null, null, null, null, null, null, null, null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_CUIT_FORMAT");
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
    }
}

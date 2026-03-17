using FluentAssertions;
using NSubstitute;
using SA.Organization.Application.Commands.Entities;
using SA.Organization.Application.Interfaces;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Entities;

public sealed class CreateEntityValidationTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly IEntityChannelRepository _channelRepository = Substitute.For<IEntityChannelRepository>();
    private readonly CreateEntityCommandHandler _sut;

    public CreateEntityValidationTests()
    {
        _sut = new CreateEntityCommandHandler(_entityRepository, _channelRepository);
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("20-1234567-1")]
    [InlineData("20-123456789-1")]
    [InlineData("XX-12345678-1")]
    [InlineData("")]
    [InlineData("invalid")]
    public async Task TP_ORG_07_08b_Create_With_Various_Invalid_CuitFormats_Throws(string invalidCuit)
    {
        var command = new CreateEntityCommand(Guid.NewGuid(), "Test", invalidCuit, "Bank", "Active",
            null, null, null, null, null, null, null, null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_CUIT_FORMAT");
    }

    [Theory]
    [InlineData("20-12345678-1")]
    [InlineData("27-87654321-0")]
    [InlineData("30-11223344-5")]
    public async Task TP_ORG_07_01b_Create_With_Valid_CuitFormats_Succeeds(string validCuit)
    {
        _entityRepository.ExistsByCuitAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _entityRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Entity.Create(
                Guid.NewGuid(), "Test", validCuit, EntityType.Bank,
                EntityStatus.Active, null, null, null, null, null, null, null));

        var command = new CreateEntityCommand(Guid.NewGuid(), "Test Entity", validCuit, "Bank", "Active",
            null, null, null, null, null, null, null, null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("")]
    [InlineData("BankX")]
    public async Task TP_ORG_07_09b_Create_With_Various_Invalid_EntityTypes_Throws(string invalidType)
    {
        var command = new CreateEntityCommand(Guid.NewGuid(), "Test", "20-12345678-1", invalidType, "Active",
            null, null, null, null, null, null, null, null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_ENTITY_TYPE");
    }

    [Fact]
    public async Task TP_ORG_07_02b_Create_With_MultipleChannels_AllCreated()
    {
        _entityRepository.ExistsByCuitAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _entityRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Entity.Create(
                Guid.NewGuid(), "Test", "20-12345678-1", EntityType.Bank,
                EntityStatus.Active, null, null, null, null, null, null, null));

        var command = new CreateEntityCommand(Guid.NewGuid(), "Test", "20-12345678-1", "Bank", "Active",
            null, null, null, null, null, null, null, new[] { "Web", "Mobile", "Api" });

        await _sut.Handle(command, CancellationToken.None);

        await _channelRepository.Received(1).AddRangeAsync(
            Arg.Is<List<EntityChannel>>(x => x.Count == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_07_13b_Create_With_InvalidChannel_AfterValidChannels_StillThrows()
    {
        _entityRepository.ExistsByCuitAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateEntityCommand(Guid.NewGuid(), "Test", "20-12345678-1", "Bank", "Active",
            null, null, null, null, null, null, null, new[] { "Web", "InvalidChannel" });

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_CHANNEL_TYPE");
    }

    [Fact]
    public async Task TP_ORG_09_02_DeleteEntity_CallsDeleteAndSave()
    {
        var entity = Entity.Create(
            Guid.NewGuid(), "Test", "20-12345678-1", EntityType.Bank,
            EntityStatus.Active, null, null, null, null, null, null, null);

        var eventPublisher = Substitute.For<IIntegrationEventPublisher>();
        var entityRepo = Substitute.For<IEntityRepository>();
        entityRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        entityRepo.HasBranchesAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new DeleteEntityCommandHandler(entityRepo, eventPublisher);
        var command = new DeleteEntityCommand(entity.Id, Guid.NewGuid());

        await handler.Handle(command, CancellationToken.None);

        await entityRepo.Received(1).DeleteAsync(entity, Arg.Any<CancellationToken>());
        await entityRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

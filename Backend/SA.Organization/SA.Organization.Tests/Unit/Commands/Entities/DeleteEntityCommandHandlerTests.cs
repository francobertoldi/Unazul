using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Commands.Entities;
using SA.Organization.Application.Interfaces;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Events;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Entities;

public sealed class DeleteEntityCommandHandlerTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly DeleteEntityCommandHandler _sut;

    public DeleteEntityCommandHandlerTests()
    {
        _sut = new DeleteEntityCommandHandler(_entityRepository, _eventPublisher);
    }

    [Fact]
    public async Task TP_ORG_09_01_Delete_Successful_CallsDeleteAndSaveAndPublish()
    {
        // Arrange
        var entity = CreateEntity();
        var deletedBy = Guid.NewGuid();
        var command = new DeleteEntityCommand(entity.Id, deletedBy);

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        _entityRepository.HasBranchesAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _entityRepository.Received(1).DeleteAsync(entity, Arg.Any<CancellationToken>());
        await _entityRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<EntityDeletedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_09_03_Delete_PublishesEntityDeletedEventWithCorrectData()
    {
        // Arrange
        var entity = CreateEntity();
        var deletedBy = Guid.NewGuid();
        var command = new DeleteEntityCommand(entity.Id, deletedBy);

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        _entityRepository.HasBranchesAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        EntityDeletedEvent? capturedEvent = null;
        await _eventPublisher.PublishAsync(
            Arg.Do<EntityDeletedEvent>(e => capturedEvent = e),
            Arg.Any<CancellationToken>());

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.TenantId.Should().Be(entity.TenantId);
        capturedEvent.EntityId.Should().Be(entity.Id);
        capturedEvent.EntityName.Should().Be(entity.Name);
        capturedEvent.DeletedBy.Should().Be(deletedBy);
        capturedEvent.CorrelationId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_ORG_09_04_Delete_HasBranches_ThrowsOrgEntityHasBranches()
    {
        // Arrange
        var entity = CreateEntity();
        var command = new DeleteEntityCommand(entity.Id, Guid.NewGuid());

        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        _entityRepository.HasBranchesAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_ENTITY_HAS_BRANCHES");
    }

    [Fact]
    public async Task TP_ORG_09_05_Delete_NotFound_ThrowsOrgEntityNotFound()
    {
        // Arrange
        var command = new DeleteEntityCommand(Guid.NewGuid(), Guid.NewGuid());

        _entityRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_ENTITY_NOT_FOUND");
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
    }
}

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Commands.Branches;
using SA.Organization.Application.Interfaces;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Events;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Branches;

public sealed class DeleteBranchCommandHandlerTests
{
    private readonly IBranchRepository _branchRepo = Substitute.For<IBranchRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly DeleteBranchCommandHandler _sut;

    public DeleteBranchCommandHandlerTests()
    {
        _sut = new DeleteBranchCommandHandler(_branchRepo, _eventPublisher);
    }

    private static Branch CreateBranch(Guid? entityId = null, Guid? tenantId = null, string code = "BR-001")
    {
        return Branch.Create(
            entityId ?? Guid.NewGuid(),
            tenantId ?? Guid.NewGuid(),
            "Test Branch",
            code,
            null, null, null, null, null, null, null, true);
    }

    [Fact]
    public async Task TP_ORG_10_03_Successful_Deletion_Calls_DeleteAsync_SaveChanges_And_Publish()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var branch = CreateBranch(entityId: entityId);
        var deletedBy = Guid.NewGuid();

        var command = new DeleteBranchCommand(entityId, branch.Id, deletedBy);

        _branchRepo.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>())
            .Returns(branch);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _branchRepo.Received(1).DeleteAsync(branch, Arg.Any<CancellationToken>());
        await _branchRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<BranchDeletedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_10_06_BranchDeletedEvent_Published_With_Correct_Fields()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var deletedBy = Guid.NewGuid();
        var branch = CreateBranch(entityId: entityId, tenantId: tenantId);

        var command = new DeleteBranchCommand(entityId, branch.Id, deletedBy);

        _branchRepo.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>())
            .Returns(branch);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<BranchDeletedEvent>(e =>
                e.TenantId == tenantId &&
                e.EntityId == entityId &&
                e.BranchId == branch.Id &&
                e.BranchName == "Test Branch" &&
                e.DeletedBy == deletedBy &&
                e.CorrelationId != Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_10_09_Branch_Not_Found_Throws_ORG_BRANCH_NOT_FOUND()
    {
        // Arrange
        var command = new DeleteBranchCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _branchRepo.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_BRANCH_NOT_FOUND");
    }

    [Fact]
    public async Task TP_ORG_10_10_Branch_Not_In_Entity_Throws_ORG_BRANCH_NOT_IN_ENTITY()
    {
        // Arrange
        var branch = CreateBranch(entityId: Guid.NewGuid());
        var differentEntityId = Guid.NewGuid();

        var command = new DeleteBranchCommand(differentEntityId, branch.Id, Guid.NewGuid());

        _branchRepo.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>())
            .Returns(branch);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_BRANCH_NOT_IN_ENTITY");
    }
}

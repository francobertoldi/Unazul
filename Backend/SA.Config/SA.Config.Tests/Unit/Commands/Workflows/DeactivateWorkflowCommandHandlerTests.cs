using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.Workflows;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.Workflows;

public sealed class DeactivateWorkflowCommandHandlerTests
{
    private readonly IWorkflowRepository _repo = Substitute.For<IWorkflowRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly DeactivateWorkflowCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public DeactivateWorkflowCommandHandlerTests()
    {
        _sut = new DeactivateWorkflowCommandHandler(_repo, _eventPublisher);
    }

    [Fact]
    public async Task TP_CFG_18_01_Deactivates_Active_Workflow()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "ActiveWF", null, UserId);
        workflow.Activate(UserId);

        _repo.GetByIdAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        var result = await _sut.Handle(new DeactivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        result.Status.Should().Be("Inactive");
        workflow.IsInactive.Should().BeTrue();
        _repo.Received(1).Update(workflow);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_18_02_Returns_409_Draft_Workflow()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "DraftWF", null, UserId);
        // Status is Draft by default

        _repo.GetByIdAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new DeactivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_NOT_ACTIVE");
    }

    [Fact]
    public async Task TP_CFG_18_03_Returns_409_Inactive_Workflow()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "InactiveWF", null, UserId);
        workflow.Activate(UserId);
        workflow.Deactivate(UserId); // Now inactive

        _repo.GetByIdAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new DeactivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_NOT_ACTIVE");
    }

    [Fact]
    public async Task TP_CFG_18_04_Returns_404_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repo.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new DeactivateWorkflowCommand(nonExistentId, UserId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_NOT_FOUND");
    }
}

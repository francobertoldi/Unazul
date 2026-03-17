using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.Workflows;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.Workflows;

public sealed class UpdateWorkflowCommandHandlerTests
{
    private readonly IWorkflowRepository _repo = Substitute.For<IWorkflowRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly UpdateWorkflowCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public UpdateWorkflowCommandHandlerTests()
    {
        _sut = new UpdateWorkflowCommandHandler(_repo, _eventPublisher);
    }

    private static WorkflowStateInput StartState() =>
        new("start", "Start", "Start", 0, 0, null, null);

    private static WorkflowStateInput EndState() =>
        new("end", "End", "End", 100, 100, null, null);

    [Fact]
    public async Task TP_CFG_13_02_Updates_Draft_Workflow()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "OldName", "old desc", UserId);
        _repo.GetByIdAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        var states = new[] { StartState(), EndState() };
        var transitions = new[] { new WorkflowTransitionInput(0, 1, "Go", null, null) };
        var command = new UpdateWorkflowCommand(workflow.Id, "NewName", "new desc", states, transitions, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("NewName");
        result.Status.Should().Be("Draft");
        _repo.Received(1).Update(workflow);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_13_03_Active_Workflow_Reverts_To_Draft()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "ActiveWF", null, UserId);
        workflow.Activate(UserId); // Make it active

        _repo.GetByIdAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        var states = new[] { StartState(), EndState() };
        var transitions = new[] { new WorkflowTransitionInput(0, 1, "Go", null, null) };
        var command = new UpdateWorkflowCommand(workflow.Id, "UpdatedName", null, states, transitions, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Draft");
        workflow.IsDraft.Should().BeTrue();
    }

    [Fact]
    public async Task TP_CFG_13_07_Returns_404_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repo.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var states = new[] { StartState(), EndState() };
        var transitions = new[] { new WorkflowTransitionInput(0, 1, "Go", null, null) };
        var command = new UpdateWorkflowCommand(nonExistentId, "Name", null, states, transitions, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_NOT_FOUND");
    }
}

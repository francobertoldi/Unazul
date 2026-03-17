using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.Workflows;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.Workflows;

public sealed class ActivateWorkflowCommandHandlerTests
{
    private readonly IWorkflowRepository _workflowRepo = Substitute.For<IWorkflowRepository>();
    private readonly IExternalServiceRepository _serviceRepo = Substitute.For<IExternalServiceRepository>();
    private readonly INotificationTemplateRepository _templateRepo = Substitute.For<INotificationTemplateRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly ActivateWorkflowCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public ActivateWorkflowCommandHandlerTests()
    {
        _sut = new ActivateWorkflowCommandHandler(
            _workflowRepo, _serviceRepo, _templateRepo, _eventPublisher);
    }

    /// <summary>
    /// Creates a valid minimal workflow graph: Start -> End.
    /// </summary>
    private static WorkflowDefinition CreateValidWorkflow()
    {
        var workflow = WorkflowDefinition.Create(TenantId, "ValidWF", null, UserId);

        var start = WorkflowState.Create(workflow.Id, TenantId, "start", "Start", FlowNodeType.Start, 0, 0);
        var end = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 100);

        workflow.States.Add(start);
        workflow.States.Add(end);

        workflow.Transitions.Add(WorkflowTransition.Create(
            workflow.Id, TenantId, start.Id, end.Id, "Go", null, null));

        return workflow;
    }

    /// <summary>
    /// Creates a workflow with a ServiceCall node referencing a given service ID.
    /// </summary>
    private static WorkflowDefinition CreateWorkflowWithServiceCall(Guid serviceId)
    {
        var workflow = WorkflowDefinition.Create(TenantId, "SvcCallWF", null, UserId);

        var start = WorkflowState.Create(workflow.Id, TenantId, "start", "Start", FlowNodeType.Start, 0, 0);
        var svcCall = WorkflowState.Create(workflow.Id, TenantId, "call_svc", "Call Service", FlowNodeType.ServiceCall, 50, 50);
        svcCall.Configs.Add(WorkflowStateConfig.Create(svcCall.Id, TenantId, "service_id", serviceId.ToString()));
        svcCall.Configs.Add(WorkflowStateConfig.Create(svcCall.Id, TenantId, "endpoint", "/api/test"));
        svcCall.Configs.Add(WorkflowStateConfig.Create(svcCall.Id, TenantId, "method", "GET"));
        var end = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 100);

        workflow.States.Add(start);
        workflow.States.Add(svcCall);
        workflow.States.Add(end);

        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, start.Id, svcCall.Id, null, null, null));
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, svcCall.Id, end.Id, null, null, null));

        return workflow;
    }

    /// <summary>
    /// Creates a workflow with a SendMessage node referencing a given template ID.
    /// </summary>
    private static WorkflowDefinition CreateWorkflowWithSendMessage(Guid templateId)
    {
        var workflow = WorkflowDefinition.Create(TenantId, "MsgWF", null, UserId);

        var start = WorkflowState.Create(workflow.Id, TenantId, "start", "Start", FlowNodeType.Start, 0, 0);
        var sendMsg = WorkflowState.Create(workflow.Id, TenantId, "notify", "Notify", FlowNodeType.SendMessage, 50, 50);
        sendMsg.Configs.Add(WorkflowStateConfig.Create(sendMsg.Id, TenantId, "channel", "email"));
        sendMsg.Configs.Add(WorkflowStateConfig.Create(sendMsg.Id, TenantId, "template_id", templateId.ToString()));
        var end = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 100);

        workflow.States.Add(start);
        workflow.States.Add(sendMsg);
        workflow.States.Add(end);

        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, start.Id, sendMsg.Id, null, null, null));
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, sendMsg.Id, end.Id, null, null, null));

        return workflow;
    }

    [Fact]
    public async Task TP_CFG_17_01_Activates_Valid_Graph()
    {
        // Arrange
        var workflow = CreateValidWorkflow();
        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        var result = await _sut.Handle(new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        result.Status.Should().Be("Active");
        workflow.IsActive.Should().BeTrue();
        _workflowRepo.Received(1).Update(workflow);
        await _workflowRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_17_03_Returns_422_No_Start_Node()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "NoStartWF", null, UserId);
        var end = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 100);
        var task = WorkflowState.Create(workflow.Id, TenantId, "task", "Task", FlowNodeType.DataCapture, 50, 50);
        task.Fields.Add(WorkflowStateField.Create(task.Id, TenantId, "f1", "text", true, 1));

        workflow.States.Add(task);
        workflow.States.Add(end);
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, task.Id, end.Id, null, null, null));

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<WorkflowValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("start node") && e.Contains("found 0"));
    }

    [Fact]
    public async Task TP_CFG_17_04_Returns_422_Multiple_Start_Nodes()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "MultiStartWF", null, UserId);
        var start1 = WorkflowState.Create(workflow.Id, TenantId, "start1", "Start1", FlowNodeType.Start, 0, 0);
        var start2 = WorkflowState.Create(workflow.Id, TenantId, "start2", "Start2", FlowNodeType.Start, 0, 50);
        var end = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 100);

        workflow.States.Add(start1);
        workflow.States.Add(start2);
        workflow.States.Add(end);
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, start1.Id, end.Id, null, null, null));
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, start2.Id, end.Id, null, null, null));

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<WorkflowValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("start node") && e.Contains("found 2"));
    }

    [Fact]
    public async Task TP_CFG_17_05_Returns_422_No_End_Node()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "NoEndWF", null, UserId);
        var start = WorkflowState.Create(workflow.Id, TenantId, "start", "Start", FlowNodeType.Start, 0, 0);
        var task = WorkflowState.Create(workflow.Id, TenantId, "task", "Task", FlowNodeType.DataCapture, 50, 50);
        task.Fields.Add(WorkflowStateField.Create(task.Id, TenantId, "f1", "text", true, 1));

        workflow.States.Add(start);
        workflow.States.Add(task);
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, start.Id, task.Id, null, null, null));

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<WorkflowValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("end node"));
    }

    [Fact]
    public async Task TP_CFG_17_06_Returns_422_Orphan_Node()
    {
        // Arrange - node with no incoming transitions (non-start)
        var workflow = WorkflowDefinition.Create(TenantId, "OrphanWF", null, UserId);
        var start = WorkflowState.Create(workflow.Id, TenantId, "start", "Start", FlowNodeType.Start, 0, 0);
        var orphan = WorkflowState.Create(workflow.Id, TenantId, "orphan", "Orphan", FlowNodeType.DataCapture, 50, 50);
        orphan.Fields.Add(WorkflowStateField.Create(orphan.Id, TenantId, "f1", "text", true, 1));
        var end = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 100);

        workflow.States.Add(start);
        workflow.States.Add(orphan);
        workflow.States.Add(end);

        // start -> end (orphan has no incoming)
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, start.Id, end.Id, null, null, null));
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, orphan.Id, end.Id, null, null, null));

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<WorkflowValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("orphan") && e.Contains("no incoming"));
    }

    [Fact]
    public async Task TP_CFG_17_08_Returns_422_Invalid_Service_Ref()
    {
        // Arrange
        var badServiceId = Guid.NewGuid();
        var workflow = CreateWorkflowWithServiceCall(badServiceId);

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);
        _serviceRepo.GetByIdAsync(badServiceId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<WorkflowValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("external service") && e.Contains("not found"));
    }

    [Fact]
    public async Task TP_CFG_17_09_Returns_422_Invalid_Template_Ref()
    {
        // Arrange
        var badTemplateId = Guid.NewGuid();
        var workflow = CreateWorkflowWithSendMessage(badTemplateId);

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);
        _templateRepo.GetByIdAsync(badTemplateId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<WorkflowValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("notification template") && e.Contains("not found"));
    }

    [Fact]
    public async Task TP_CFG_17_10_Returns_422_DataCapture_No_Fields()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "NoCaptureFieldsWF", null, UserId);
        var start = WorkflowState.Create(workflow.Id, TenantId, "start", "Start", FlowNodeType.Start, 0, 0);
        var capture = WorkflowState.Create(workflow.Id, TenantId, "capture", "Capture", FlowNodeType.DataCapture, 50, 50);
        // No fields added to capture
        var end = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 100);

        workflow.States.Add(start);
        workflow.States.Add(capture);
        workflow.States.Add(end);

        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, start.Id, capture.Id, null, null, null));
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, capture.Id, end.Id, null, null, null));

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<WorkflowValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("data_capture") && e.Contains("at least 1 field"));
    }

    [Fact]
    public async Task TP_CFG_17_11_Returns_409_Already_Active()
    {
        // Arrange
        var workflow = CreateValidWorkflow();
        workflow.Activate(UserId); // Already active

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_ALREADY_ACTIVE");
    }

    [Fact]
    public async Task TP_CFG_17_14_Returns_Multiple_Errors()
    {
        // Arrange - no start, no end, has orphan DataCapture with no fields
        var workflow = WorkflowDefinition.Create(TenantId, "MultiErrorWF", null, UserId);
        var task1 = WorkflowState.Create(workflow.Id, TenantId, "task1", "Task1", FlowNodeType.DataCapture, 50, 50);
        // No fields for DataCapture
        var task2 = WorkflowState.Create(workflow.Id, TenantId, "task2", "Task2", FlowNodeType.DataCapture, 50, 100);
        // No fields for DataCapture

        workflow.States.Add(task1);
        workflow.States.Add(task2);

        // task1 -> task2 but task1 has no incoming, task2 has no outgoing
        workflow.Transitions.Add(WorkflowTransition.Create(workflow.Id, TenantId, task1.Id, task2.Id, null, null, null));

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<WorkflowValidationException>();
        ex.Which.Errors.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task TP_CFG_17_15_Increments_Version()
    {
        // Arrange
        var workflow = CreateValidWorkflow();
        workflow.Version.Should().Be(0);

        _workflowRepo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        var result = await _sut.Handle(new ActivateWorkflowCommand(workflow.Id, UserId), CancellationToken.None);

        // Assert
        result.Version.Should().Be(1);
        workflow.Version.Should().Be(1);
    }
}

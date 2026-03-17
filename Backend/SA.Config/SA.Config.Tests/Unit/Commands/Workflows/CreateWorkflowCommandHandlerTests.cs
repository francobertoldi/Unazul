using FluentAssertions;
using NSubstitute;
using SA.Config.Application.Commands.Workflows;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.Workflows;

public sealed class CreateWorkflowCommandHandlerTests
{
    private readonly IWorkflowRepository _repo = Substitute.For<IWorkflowRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly CreateWorkflowCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public CreateWorkflowCommandHandlerTests()
    {
        _sut = new CreateWorkflowCommandHandler(_repo, _eventPublisher);
    }

    private static WorkflowStateInput StartState(string name = "start") =>
        new(name, "Start", "Start", 0, 0, null, null);

    private static WorkflowStateInput EndState(string name = "end") =>
        new(name, "End", "End", 100, 100, null, null);

    private static WorkflowStateInput ServiceCallState(string name = "call_api") =>
        new(name, "Call API", "ServiceCall", 50, 50,
            new[]
            {
                new StateConfigInput("service_id", Guid.NewGuid().ToString()),
                new StateConfigInput("endpoint", "/api/test"),
                new StateConfigInput("method", "GET")
            }, null);

    private static WorkflowStateInput DecisionState(string name = "decision") =>
        new(name, "Decision", "Decision", 50, 50,
            new[] { new StateConfigInput("condition", "amount > 100") }, null);

    private static WorkflowStateInput SendMessageState(string name = "notify") =>
        new(name, "Notify", "SendMessage", 50, 50,
            new[]
            {
                new StateConfigInput("channel", "email"),
                new StateConfigInput("template_id", Guid.NewGuid().ToString())
            }, null);

    private static WorkflowStateInput TimerState(string name = "wait") =>
        new(name, "Wait", "Timer", 50, 50,
            new[] { new StateConfigInput("timer_minutes", "30") }, null);

    private static WorkflowStateInput DataCaptureState(string name = "capture") =>
        new(name, "Capture Data", "DataCapture", 50, 50, null,
            new[] { new StateFieldInput("customer_name", "text", true, 1) });

    [Fact]
    public async Task TP_CFG_13_01_Creates_Draft_Workflow()
    {
        // Arrange
        var states = new[] { StartState(), EndState() };
        var transitions = new[] { new WorkflowTransitionInput(0, 1, "Go", null, null) };
        var command = new CreateWorkflowCommand(TenantId, "My Workflow", "desc", states, transitions, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("My Workflow");
        result.Status.Should().Be("Draft");
        result.Version.Should().Be(0);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_13_05_Returns_422_Empty_Name()
    {
        // Arrange
        var states = new[] { StartState(), EndState() };
        var transitions = new[] { new WorkflowTransitionInput(0, 1, "Go", null, null) };
        var command = new CreateWorkflowCommand(TenantId, "", null, states, transitions, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_NAME_REQUIRED");
    }

    [Fact]
    public async Task TP_CFG_13_06_Returns_422_Empty_States()
    {
        // Arrange
        var command = new CreateWorkflowCommand(
            TenantId, "WF", null, Array.Empty<WorkflowStateInput>(),
            Array.Empty<WorkflowTransitionInput>(), UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_STATES_REQUIRED");
    }

    [Fact]
    public async Task TP_CFG_14_01_to_05_Validates_Node_Configs_Per_Type()
    {
        // ServiceCall missing service_id
        var badServiceCall = new WorkflowStateInput("bad_call", "Bad Call", "ServiceCall", 50, 50, null, null);
        var command1 = new CreateWorkflowCommand(TenantId, "WF", null,
            new[] { StartState(), badServiceCall, EndState() },
            new[] { new WorkflowTransitionInput(0, 1, null, null, null), new WorkflowTransitionInput(1, 2, null, null, null) },
            UserId);

        Func<Task> act1 = async () => await _sut.Handle(command1, CancellationToken.None);
        await act1.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*service_call requires service_id*");

        // Decision missing condition
        var badDecision = new WorkflowStateInput("bad_dec", "Bad Dec", "Decision", 50, 50, null, null);
        var command2 = new CreateWorkflowCommand(TenantId, "WF", null,
            new[] { StartState(), badDecision, EndState() },
            new[] { new WorkflowTransitionInput(0, 1, null, null, null), new WorkflowTransitionInput(1, 2, null, null, null) },
            UserId);

        Func<Task> act2 = async () => await _sut.Handle(command2, CancellationToken.None);
        await act2.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*decision requires condition*");

        // SendMessage missing channel
        var badSendMsg = new WorkflowStateInput("bad_msg", "Bad Msg", "SendMessage", 50, 50, null, null);
        var command3 = new CreateWorkflowCommand(TenantId, "WF", null,
            new[] { StartState(), badSendMsg, EndState() },
            new[] { new WorkflowTransitionInput(0, 1, null, null, null), new WorkflowTransitionInput(1, 2, null, null, null) },
            UserId);

        Func<Task> act3 = async () => await _sut.Handle(command3, CancellationToken.None);
        await act3.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*send_message requires channel*");

        // Timer missing timer_minutes
        var badTimer = new WorkflowStateInput("bad_timer", "Bad Timer", "Timer", 50, 50, null, null);
        var command4 = new CreateWorkflowCommand(TenantId, "WF", null,
            new[] { StartState(), badTimer, EndState() },
            new[] { new WorkflowTransitionInput(0, 1, null, null, null), new WorkflowTransitionInput(1, 2, null, null, null) },
            UserId);

        Func<Task> act4 = async () => await _sut.Handle(command4, CancellationToken.None);
        await act4.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*timer requires timer_minutes*");

        // DataCapture missing fields
        var badCapture = new WorkflowStateInput("bad_capture", "Bad Capture", "DataCapture", 50, 50, null, null);
        var command5 = new CreateWorkflowCommand(TenantId, "WF", null,
            new[] { StartState(), badCapture, EndState() },
            new[] { new WorkflowTransitionInput(0, 1, null, null, null), new WorkflowTransitionInput(1, 2, null, null, null) },
            UserId);

        Func<Task> act5 = async () => await _sut.Handle(command5, CancellationToken.None);
        await act5.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*data_capture requires at least 1 field*");

        // Verify valid nodes work
        var validCommand = new CreateWorkflowCommand(TenantId, "ValidWF", null,
            new[] { StartState(), ServiceCallState(), DecisionState(), SendMessageState(), TimerState(), DataCaptureState(), EndState() },
            new[]
            {
                new WorkflowTransitionInput(0, 1, null, null, null),
                new WorkflowTransitionInput(1, 2, null, null, null),
                new WorkflowTransitionInput(2, 3, null, null, null),
                new WorkflowTransitionInput(3, 4, null, null, null),
                new WorkflowTransitionInput(4, 5, null, null, null),
                new WorkflowTransitionInput(5, 6, null, null, null)
            },
            UserId);

        var validResult = await _sut.Handle(validCommand, CancellationToken.None);
        validResult.Name.Should().Be("ValidWF");
    }

    [Fact]
    public async Task TP_CFG_16_04_Returns_422_SelfRef_Transition()
    {
        // Arrange
        var states = new[] { StartState(), EndState() };
        var transitions = new[] { new WorkflowTransitionInput(0, 0, "Self", null, null) };
        var command = new CreateWorkflowCommand(TenantId, "WF", null, states, transitions, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_SELF_REFERENCING_TRANSITION");
    }

    [Fact]
    public async Task TP_CFG_16_05_Returns_422_Duplicate_Transition()
    {
        // Arrange
        var states = new[] { StartState(), EndState() };
        var transitions = new[]
        {
            new WorkflowTransitionInput(0, 1, "First", null, null),
            new WorkflowTransitionInput(0, 1, "Duplicate", null, null)
        };
        var command = new CreateWorkflowCommand(TenantId, "WF", null, states, transitions, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_DUPLICATE_TRANSITION");
    }
}

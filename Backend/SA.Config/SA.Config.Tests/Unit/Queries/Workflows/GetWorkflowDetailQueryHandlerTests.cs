using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Queries.Workflows;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using Xunit;

namespace SA.Config.Tests.Unit.Queries.Workflows;

public sealed class GetWorkflowDetailQueryHandlerTests
{
    private readonly IWorkflowRepository _repo = Substitute.For<IWorkflowRepository>();
    private readonly GetWorkflowDetailQueryHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public GetWorkflowDetailQueryHandlerTests()
    {
        _sut = new GetWorkflowDetailQueryHandler(_repo);
    }

    [Fact]
    public async Task TP_CFG_13_04_Returns_Full_Workflow_With_States_And_Transitions()
    {
        // Arrange
        var workflow = WorkflowDefinition.Create(TenantId, "FullWorkflow", "desc", Guid.NewGuid());

        var startState = WorkflowState.Create(workflow.Id, TenantId, "start", "Start", FlowNodeType.Start, 0, 0);
        var endState = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 100);
        startState.Configs.Add(WorkflowStateConfig.Create(startState.Id, TenantId, "key1", "val1"));
        endState.Fields.Add(WorkflowStateField.Create(endState.Id, TenantId, "field1", "text", true, 1));

        workflow.States.Add(startState);
        workflow.States.Add(endState);

        var transition = WorkflowTransition.Create(
            workflow.Id, TenantId, startState.Id, endState.Id, "Go", null, null);
        workflow.Transitions.Add(transition);

        _repo.GetByIdFullAsync(workflow.Id, Arg.Any<CancellationToken>())
            .Returns(workflow);

        // Act
        var result = await _sut.Handle(new GetWorkflowDetailQuery(workflow.Id), CancellationToken.None);

        // Assert
        result.Id.Should().Be(workflow.Id);
        result.Name.Should().Be("FullWorkflow");
        result.States.Should().HaveCount(2);
        result.Transitions.Should().HaveCount(1);

        var startDto = result.States.First(s => s.Type == "Start");
        startDto.Configs.Should().HaveCount(1);
        startDto.Configs[0].Key.Should().Be("key1");

        var endDto = result.States.First(s => s.Type == "End");
        endDto.Fields.Should().HaveCount(1);
        endDto.Fields[0].FieldName.Should().Be("field1");

        result.Transitions[0].FromStateId.Should().Be(startState.Id);
        result.Transitions[0].ToStateId.Should().Be(endState.Id);
    }

    [Fact]
    public async Task TP_CFG_13_07_Returns_404_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repo.GetByIdFullAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new GetWorkflowDetailQuery(nonExistentId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WORKFLOW_NOT_FOUND");
    }
}

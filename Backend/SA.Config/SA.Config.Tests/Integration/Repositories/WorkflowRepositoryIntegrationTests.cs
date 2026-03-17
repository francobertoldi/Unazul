using FluentAssertions;
using SA.Config.DataAccess.EntityFramework.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using SA.Config.Tests.Integration.Fixtures;
using Xunit;

namespace SA.Config.Tests.Integration.Repositories;

public sealed class WorkflowRepositoryIntegrationTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public WorkflowRepositoryIntegrationTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Creates_Workflow_With_States_And_Transitions()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new WorkflowRepository(db);

        var workflow = WorkflowDefinition.Create(TenantId, "Approval Flow", "A test flow", UserId);

        var startState = WorkflowState.Create(workflow.Id, TenantId, "start", "Start", FlowNodeType.Start, 0, 0);
        var endState = WorkflowState.Create(workflow.Id, TenantId, "end", "End", FlowNodeType.End, 100, 0);

        workflow.States.Add(startState);
        workflow.States.Add(endState);

        var transition = WorkflowTransition.Create(
            workflow.Id, TenantId, startState.Id, endState.Id, "approve", null, 24);
        workflow.Transitions.Add(transition);

        // Act
        await repo.AddAsync(workflow);
        await repo.SaveChangesAsync();

        // Assert
        var loaded = await repo.GetByIdAsync(workflow.Id);
        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Approval Flow");
        loaded.Status.Should().Be(WorkflowStatus.Draft);
    }

    [Fact]
    public async Task GetByIdFullAsync_Loads_Everything()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new WorkflowRepository(db);

        var workflow = WorkflowDefinition.Create(TenantId, "Full Load Flow", null, UserId);

        var state = WorkflowState.Create(workflow.Id, TenantId, "data_capture", "Capture Data", FlowNodeType.DataCapture, 50, 50);
        var config = WorkflowStateConfig.Create(state.Id, TenantId, "form_id", "form_123");
        state.Configs.Add(config);

        var field = WorkflowStateField.Create(state.Id, TenantId, "name", "text", true, 1);
        state.Fields.Add(field);

        workflow.States.Add(state);

        await repo.AddAsync(workflow);
        await repo.SaveChangesAsync();

        // Act
        var loaded = await repo.GetByIdFullAsync(workflow.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.States.Should().HaveCount(1);

        var loadedState = loaded.States.First();
        loadedState.Configs.Should().HaveCount(1);
        loadedState.Configs.First().Key.Should().Be("form_id");
        loadedState.Fields.Should().HaveCount(1);
        loadedState.Fields.First().FieldName.Should().Be("name");
    }

    [Fact]
    public async Task ReplaceChildrenAsync_Replaces_All()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new WorkflowRepository(db);

        var workflow = WorkflowDefinition.Create(TenantId, "Replace Flow", null, UserId);
        var oldState = WorkflowState.Create(workflow.Id, TenantId, "old_state", "Old", FlowNodeType.Start, 0, 0);
        workflow.States.Add(oldState);

        await repo.AddAsync(workflow);
        await repo.SaveChangesAsync();

        // Act - replace with new states and transitions
        var newStart = WorkflowState.Create(workflow.Id, TenantId, "new_start", "New Start", FlowNodeType.Start, 0, 0);
        var newEnd = WorkflowState.Create(workflow.Id, TenantId, "new_end", "New End", FlowNodeType.End, 100, 0);
        var newTransition = WorkflowTransition.Create(workflow.Id, TenantId, newStart.Id, newEnd.Id, "next", null, null);

        await repo.ReplaceChildrenAsync(workflow.Id, TenantId,
            new[] { newStart, newEnd },
            new[] { newTransition });
        await repo.SaveChangesAsync();

        // Assert
        var loaded = await repo.GetByIdFullAsync(workflow.Id);
        loaded.Should().NotBeNull();
        loaded!.States.Should().HaveCount(2);
        loaded.States.Should().Contain(s => s.Name == "new_start");
        loaded.States.Should().NotContain(s => s.Name == "old_state");
        loaded.Transitions.Should().HaveCount(1);
        loaded.Transitions.First().Label.Should().Be("next");
    }
}

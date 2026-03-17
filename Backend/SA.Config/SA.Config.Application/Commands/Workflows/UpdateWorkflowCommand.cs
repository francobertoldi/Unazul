using Mediator;
using SA.Config.Application.Dtos.Workflows;

namespace SA.Config.Application.Commands.Workflows;

public readonly record struct UpdateWorkflowCommand(
    Guid Id,
    string Name,
    string? Description,
    WorkflowStateInput[] States,
    WorkflowTransitionInput[] Transitions,
    Guid UpdatedBy) : ICommand<WorkflowSummaryDto>;

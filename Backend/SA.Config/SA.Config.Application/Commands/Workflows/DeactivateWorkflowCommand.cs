using Mediator;
using SA.Config.Application.Dtos.Workflows;

namespace SA.Config.Application.Commands.Workflows;

public readonly record struct DeactivateWorkflowCommand(
    Guid Id,
    Guid UpdatedBy) : ICommand<WorkflowSummaryDto>;

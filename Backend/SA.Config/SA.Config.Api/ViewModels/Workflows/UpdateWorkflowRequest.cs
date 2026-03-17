namespace SA.Config.Api.ViewModels.Workflows;

public sealed record UpdateWorkflowRequest(
    string Name,
    string? Description,
    WorkflowStateRequest[] States,
    WorkflowTransitionRequest[] Transitions);

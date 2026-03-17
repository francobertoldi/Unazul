namespace SA.Config.Application.Commands.Workflows;

public sealed class WorkflowValidationException(IReadOnlyList<string> errors)
    : Exception("Workflow validation failed.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}

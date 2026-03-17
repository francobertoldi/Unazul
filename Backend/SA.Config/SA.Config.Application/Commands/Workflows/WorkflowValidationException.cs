using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Workflows;

public sealed class WorkflowValidationException(IReadOnlyList<string> errors)
    : ValidationException("WORKFLOW_VALIDATION_FAILED", "La validacion del workflow fallo.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}

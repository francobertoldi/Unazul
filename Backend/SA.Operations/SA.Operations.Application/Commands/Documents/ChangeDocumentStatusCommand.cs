using Mediator;

namespace SA.Operations.Application.Commands.Documents;

public readonly record struct ChangeDocumentStatusCommand(
    Guid DocumentId,
    Guid TenantId,
    string NewStatus,
    Guid UpdatedBy) : ICommand<ChangeDocumentStatusResult>;

public sealed record ChangeDocumentStatusResult(Guid Id, string Status);

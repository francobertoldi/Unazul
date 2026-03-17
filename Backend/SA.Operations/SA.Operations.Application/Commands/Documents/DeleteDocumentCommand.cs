using Mediator;

namespace SA.Operations.Application.Commands.Documents;

public readonly record struct DeleteDocumentCommand(
    Guid DocumentId,
    Guid TenantId) : ICommand<DeleteDocumentResult>;

public sealed record DeleteDocumentResult(Guid Id);

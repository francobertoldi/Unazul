using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Documents;

public sealed class ChangeDocumentStatusCommandHandler(
    IDocumentRepository documentRepository) : ICommandHandler<ChangeDocumentStatusCommand, ChangeDocumentStatusResult>
{
    public async ValueTask<ChangeDocumentStatusResult> Handle(ChangeDocumentStatusCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<DocumentStatus>(command.NewStatus, true, out var newStatus))
            throw new ValidationException("OPS_INVALID_DOCUMENT_STATUS", "Estado de documento invalido.");

        var document = await documentRepository.GetByIdAsync(command.DocumentId, ct);
        if (document is null || document.TenantId != command.TenantId)
            throw new NotFoundException("OPS_DOCUMENT_NOT_FOUND", "Documento no encontrado.");

        document.ChangeStatus(newStatus, command.UpdatedBy);

        documentRepository.Update(document);
        await documentRepository.SaveChangesAsync(ct);

        return new ChangeDocumentStatusResult(document.Id, document.Status.ToString());
    }
}

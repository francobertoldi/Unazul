using Mediator;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Documents;

public sealed class DeleteDocumentCommandHandler(
    IDocumentRepository documentRepository,
    IFileStorageService fileStorageService) : ICommandHandler<DeleteDocumentCommand, DeleteDocumentResult>
{
    public async ValueTask<DeleteDocumentResult> Handle(DeleteDocumentCommand command, CancellationToken ct)
    {
        var document = await documentRepository.GetByIdAsync(command.DocumentId, ct);
        if (document is null || document.TenantId != command.TenantId)
            throw new NotFoundException("OPS_DOCUMENT_NOT_FOUND", "Documento no encontrado.");

        // Delete file from storage
        await fileStorageService.DeleteDocumentAsync(document.FileUrl, ct);

        // Delete record
        documentRepository.Delete(document);
        await documentRepository.SaveChangesAsync(ct);

        return new DeleteDocumentResult(document.Id);
    }
}

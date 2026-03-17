using Mediator;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Documents;

public sealed class UploadDocumentCommandHandler(
    IApplicationRepository applicationRepository,
    IDocumentRepository documentRepository,
    IFileStorageService fileStorageService) : ICommandHandler<UploadDocumentCommand, UploadDocumentResult>
{
    public async ValueTask<UploadDocumentResult> Handle(UploadDocumentCommand command, CancellationToken ct)
    {
        var app = await applicationRepository.GetByIdAsync(command.ApplicationId, ct);
        if (app is null || app.TenantId != command.TenantId)
            throw new NotFoundException("OPS_APPLICATION_NOT_FOUND", "Solicitud no encontrada.");

        var documentId = Guid.CreateVersion7();

        // Upload file to storage
        var fileUrl = await fileStorageService.SaveDocumentAsync(
            command.TenantId,
            command.ApplicationId,
            documentId,
            command.OriginalFileName,
            command.FileContent,
            ct);

        // Create document record
        var document = ApplicationDocument.Create(
            command.ApplicationId,
            command.TenantId,
            command.Name,
            command.DocumentType,
            fileUrl,
            command.CreatedBy);

        await documentRepository.AddAsync(document, ct);
        await documentRepository.SaveChangesAsync(ct);

        return new UploadDocumentResult(document.Id, document.Name, fileUrl);
    }
}

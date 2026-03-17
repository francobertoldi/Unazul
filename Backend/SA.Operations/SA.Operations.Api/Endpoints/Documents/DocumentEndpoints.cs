using SA.Operations.Api.Mappers.Documents;
using SA.Operations.Api.ViewModels.Documents;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Operations.Api.Endpoints.Documents;

public static class DocumentEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/applications/{applicationId:guid}/documents")
            .WithTags("Documents")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-OPS-10: Upload document
        group.MapPost("/", async (
            Guid applicationId,
            IFormFile file,
            string documentType,
            IApplicationRepository applicationRepository,
            IDocumentRepository repository,
            IFileStorageService storageService,
            ICurrentUser currentUser) =>
        {
            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var documentId = Guid.CreateVersion7();

            await using var stream = file.OpenReadStream();
            var filePath = await storageService.SaveDocumentAsync(
                currentUser.TenantId,
                applicationId,
                documentId,
                file.FileName,
                stream);

            var document = ApplicationDocument.Create(
                applicationId,
                currentUser.TenantId,
                file.FileName,
                documentType,
                filePath,
                currentUser.UserId);

            await repository.AddAsync(document);
            await repository.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/applications/{applicationId}/documents/{document.Id}",
                DocumentMapper.ToResponse(document));
        })
        .DisableAntiforgery()
        .Produces<DocumentResponse>(201)
        .Produces<ErrorResponse>(404);

        // Change document status
        group.MapPut("/{id:guid}/status", async (
            Guid applicationId,
            Guid id,
            ChangeDocumentStatusRequest request,
            IApplicationRepository applicationRepository,
            IDocumentRepository repository,
            ICurrentUser currentUser) =>
        {
            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var document = await repository.GetByIdAsync(id);
            if (document is null || document.ApplicationId != applicationId)
                return Results.Json(
                    new ErrorResponse("Documento no encontrado.", "OPS_DOCUMENT_NOT_FOUND"),
                    statusCode: 404);

            document.ChangeStatus(request.Status, currentUser.UserId);
            repository.Update(document);
            await repository.SaveChangesAsync();

            return Results.Ok(DocumentMapper.ToResponse(document));
        })
        .Produces<DocumentResponse>(200)
        .Produces<ErrorResponse>(404);

        // Delete document
        group.MapDelete("/{id:guid}", async (
            Guid applicationId,
            Guid id,
            IApplicationRepository applicationRepository,
            IDocumentRepository repository,
            IFileStorageService storageService) =>
        {
            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var document = await repository.GetByIdAsync(id);
            if (document is null || document.ApplicationId != applicationId)
                return Results.Json(
                    new ErrorResponse("Documento no encontrado.", "OPS_DOCUMENT_NOT_FOUND"),
                    statusCode: 404);

            await storageService.DeleteDocumentAsync(document.FileUrl);
            repository.Delete(document);
            await repository.SaveChangesAsync();

            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);
    }
}

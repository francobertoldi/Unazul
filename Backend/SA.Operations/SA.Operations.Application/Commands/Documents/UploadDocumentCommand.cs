using Mediator;

namespace SA.Operations.Application.Commands.Documents;

public readonly record struct UploadDocumentCommand(
    Guid ApplicationId,
    Guid TenantId,
    string Name,
    string DocumentType,
    string OriginalFileName,
    Stream FileContent,
    Guid CreatedBy) : ICommand<UploadDocumentResult>;

public sealed record UploadDocumentResult(Guid Id, string Name, string FileUrl);

using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Documents;

public sealed record DocumentResponse(
    Guid Id,
    string Name,
    string DocumentType,
    string FileUrl,
    DocumentStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedBy,
    Guid? UpdatedBy);

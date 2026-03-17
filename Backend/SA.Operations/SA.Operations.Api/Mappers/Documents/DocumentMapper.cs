using SA.Operations.Api.ViewModels.Documents;
using SA.Operations.Domain.Entities;

namespace SA.Operations.Api.Mappers.Documents;

public static class DocumentMapper
{
    public static DocumentResponse ToResponse(ApplicationDocument doc)
    {
        return new DocumentResponse(
            doc.Id,
            doc.Name,
            doc.DocumentType,
            doc.FileUrl,
            doc.Status,
            doc.CreatedAt,
            doc.UpdatedAt,
            doc.CreatedBy,
            doc.UpdatedBy);
    }
}

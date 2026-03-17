using SA.Operations.Domain.Enums;

namespace SA.Operations.Domain.Entities;

public sealed class ApplicationDocument
{
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string DocumentType { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private ApplicationDocument() { }

    public static ApplicationDocument Create(
        Guid applicationId,
        Guid tenantId,
        string name,
        string documentType,
        string fileUrl,
        Guid createdBy)
    {
        var now = DateTime.UtcNow;
        return new ApplicationDocument
        {
            Id = Guid.CreateVersion7(),
            ApplicationId = applicationId,
            TenantId = tenantId,
            Name = name,
            DocumentType = documentType,
            FileUrl = fileUrl,
            Status = DocumentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy
        };
    }

    public void ChangeStatus(DocumentStatus status, Guid updatedBy)
    {
        Status = status;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

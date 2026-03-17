using SA.Operations.Domain.Enums;

namespace SA.Operations.Domain.Entities;

public sealed class Application
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid ApplicantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public Guid PlanId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string PlanName { get; private set; } = string.Empty;
    public ApplicationStatus Status { get; private set; }
    public string? WorkflowStage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    // Navigation properties
    public Applicant? Applicant { get; private set; }

    private Application() { }

    public static Application Create(
        Guid tenantId,
        Guid entityId,
        Guid applicantId,
        string code,
        Guid productId,
        Guid planId,
        string productName,
        string planName,
        Guid createdBy)
    {
        var now = DateTime.UtcNow;
        return new Application
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            EntityId = entityId,
            ApplicantId = applicantId,
            Code = code,
            ProductId = productId,
            PlanId = planId,
            ProductName = productName,
            PlanName = planName,
            Status = ApplicationStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void UpdateDraft(
        Guid? entityId,
        Guid? productId,
        Guid? planId,
        string? productName,
        string? planName,
        Guid updatedBy)
    {
        if (entityId.HasValue) EntityId = entityId.Value;
        if (productId.HasValue) ProductId = productId.Value;
        if (planId.HasValue) PlanId = planId.Value;
        if (productName is not null) ProductName = productName;
        if (planName is not null) PlanName = planName;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool TransitionStatus(ApplicationStatus newStatus, Guid updatedBy)
    {
        if (!ApplicationStateMachine.IsValidTransition(Status, newStatus))
            return false;

        Status = newStatus;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
}

using SA.Config.Domain.Enums;

namespace SA.Config.Domain.Entities;

public sealed class WorkflowDefinition
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public ICollection<WorkflowState> States { get; private set; } = [];
    public ICollection<WorkflowTransition> Transitions { get; private set; } = [];

    private WorkflowDefinition() { }

    public static WorkflowDefinition Create(
        Guid tenantId,
        string name,
        string? description,
        Guid createdBy)
    {
        var now = DateTime.UtcNow;
        return new WorkflowDefinition
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            Status = WorkflowStatus.Draft,
            Version = 0,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void UpdateDraft(string name, string? description, Guid updatedBy)
    {
        if (Status == WorkflowStatus.Active)
        {
            Status = WorkflowStatus.Draft;
        }

        Name = name;
        Description = description;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(Guid updatedBy)
    {
        Status = WorkflowStatus.Active;
        Version++;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(Guid updatedBy)
    {
        Status = WorkflowStatus.Inactive;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsDraft => Status == WorkflowStatus.Draft;
    public bool IsActive => Status == WorkflowStatus.Active;
    public bool IsInactive => Status == WorkflowStatus.Inactive;
}

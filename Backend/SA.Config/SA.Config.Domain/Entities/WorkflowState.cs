using SA.Config.Domain.Enums;

namespace SA.Config.Domain.Entities;

public sealed class WorkflowState
{
    public Guid Id { get; private set; }
    public Guid WorkflowId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Label { get; private set; }
    public FlowNodeType Type { get; private set; }
    public decimal PositionX { get; private set; }
    public decimal PositionY { get; private set; }

    public ICollection<WorkflowStateConfig> Configs { get; private set; } = [];
    public ICollection<WorkflowStateField> Fields { get; private set; } = [];

    private WorkflowState() { }

    public static WorkflowState Create(
        Guid workflowId,
        Guid tenantId,
        string name,
        string? label,
        FlowNodeType type,
        decimal positionX,
        decimal positionY)
    {
        return new WorkflowState
        {
            Id = Guid.CreateVersion7(),
            WorkflowId = workflowId,
            TenantId = tenantId,
            Name = name,
            Label = label,
            Type = type,
            PositionX = positionX,
            PositionY = positionY
        };
    }
}

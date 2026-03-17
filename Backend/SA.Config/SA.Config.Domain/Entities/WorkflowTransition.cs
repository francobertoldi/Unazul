namespace SA.Config.Domain.Entities;

public sealed class WorkflowTransition
{
    public Guid Id { get; private set; }
    public Guid WorkflowId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid FromStateId { get; private set; }
    public Guid ToStateId { get; private set; }
    public string? Label { get; private set; }
    public string? Condition { get; private set; }
    public int? SlaHours { get; private set; }

    private WorkflowTransition() { }

    public static WorkflowTransition Create(
        Guid workflowId,
        Guid tenantId,
        Guid fromStateId,
        Guid toStateId,
        string? label,
        string? condition,
        int? slaHours)
    {
        return new WorkflowTransition
        {
            Id = Guid.CreateVersion7(),
            WorkflowId = workflowId,
            TenantId = tenantId,
            FromStateId = fromStateId,
            ToStateId = toStateId,
            Label = label,
            Condition = condition,
            SlaHours = slaHours
        };
    }
}

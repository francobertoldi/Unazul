namespace SA.Config.Domain.Entities;

public sealed class WorkflowStateConfig
{
    public Guid Id { get; private set; }
    public Guid StateId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    private WorkflowStateConfig() { }

    public static WorkflowStateConfig Create(
        Guid stateId,
        Guid tenantId,
        string key,
        string value)
    {
        return new WorkflowStateConfig
        {
            Id = Guid.CreateVersion7(),
            StateId = stateId,
            TenantId = tenantId,
            Key = key,
            Value = value
        };
    }
}

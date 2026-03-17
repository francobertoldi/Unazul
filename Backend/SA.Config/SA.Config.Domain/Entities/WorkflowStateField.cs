namespace SA.Config.Domain.Entities;

public sealed class WorkflowStateField
{
    public Guid Id { get; private set; }
    public Guid StateId { get; private set; }
    public Guid TenantId { get; private set; }
    public string FieldName { get; private set; } = string.Empty;
    public string FieldType { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }

    private WorkflowStateField() { }

    public static WorkflowStateField Create(
        Guid stateId,
        Guid tenantId,
        string fieldName,
        string fieldType,
        bool isRequired,
        int sortOrder)
    {
        return new WorkflowStateField
        {
            Id = Guid.CreateVersion7(),
            StateId = stateId,
            TenantId = tenantId,
            FieldName = fieldName,
            FieldType = fieldType,
            IsRequired = isRequired,
            SortOrder = sortOrder
        };
    }
}

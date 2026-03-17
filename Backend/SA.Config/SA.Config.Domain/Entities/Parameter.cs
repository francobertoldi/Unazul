using SA.Config.Domain.Enums;

namespace SA.Config.Domain.Entities;

public sealed class Parameter
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid GroupId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public ParameterType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ParentKey { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public ICollection<ParameterOption> Options { get; private set; } = [];

    private Parameter() { }

    public static Parameter Create(
        Guid tenantId,
        Guid groupId,
        string key,
        string value,
        ParameterType type,
        string description,
        string? parentKey,
        Guid updatedBy)
    {
        return new Parameter
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            GroupId = groupId,
            Key = key,
            Value = value,
            Type = type,
            Description = description,
            ParentKey = parentKey,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = updatedBy
        };
    }

    public void UpdateValue(string value, List<ParameterOption>? options, Guid updatedBy)
    {
        Value = value;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        if (options is not null)
        {
            Options = options;
        }
    }
}

namespace SA.Config.Domain.Entities;

public sealed class ParameterOption
{
    public Guid Id { get; private set; }
    public Guid ParameterId { get; private set; }
    public Guid TenantId { get; private set; }
    public string OptionValue { get; private set; } = string.Empty;
    public string OptionLabel { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private ParameterOption() { }

    public static ParameterOption Create(
        Guid parameterId,
        Guid tenantId,
        string optionValue,
        string optionLabel,
        int sortOrder)
    {
        return new ParameterOption
        {
            Id = Guid.CreateVersion7(),
            ParameterId = parameterId,
            TenantId = tenantId,
            OptionValue = optionValue,
            OptionLabel = optionLabel,
            SortOrder = sortOrder
        };
    }
}

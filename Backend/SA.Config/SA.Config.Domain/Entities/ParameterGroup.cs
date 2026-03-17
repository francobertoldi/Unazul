namespace SA.Config.Domain.Entities;

public sealed class ParameterGroup
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Icon { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private ParameterGroup() { }

    public static ParameterGroup Create(
        string code,
        string name,
        string category,
        string icon,
        int sortOrder)
    {
        return new ParameterGroup
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Category = category,
            Icon = icon,
            SortOrder = sortOrder
        };
    }
}

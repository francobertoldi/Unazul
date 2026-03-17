namespace SA.Catalog.Domain.Entities;

public sealed class ProductRequirement
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public bool IsMandatory { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static readonly string[] ValidTypes = ["document", "data", "validation"];

    private ProductRequirement() { }

    public static ProductRequirement Create(Guid productId, Guid tenantId, string name, string type, bool isMandatory, string? description)
    {
        return new ProductRequirement
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            TenantId = tenantId,
            Name = name,
            Type = type,
            IsMandatory = isMandatory,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string type, bool isMandatory, string? description)
    {
        Name = name;
        Type = type;
        IsMandatory = isMandatory;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}

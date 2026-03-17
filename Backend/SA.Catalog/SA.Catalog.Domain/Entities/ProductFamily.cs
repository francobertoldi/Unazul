namespace SA.Catalog.Domain.Entities;

public sealed class ProductFamily
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    private ProductFamily() { }

    public static ProductFamily Create(Guid tenantId, string code, string description, Guid userId)
    {
        return new ProductFamily
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Code = code,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };
    }

    public void Update(string description, Guid userId)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
}

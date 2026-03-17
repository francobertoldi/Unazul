using Shared.Contract.Enums;

namespace SA.Catalog.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProductStatus Status { get; private set; }
    public DateOnly ValidFrom { get; private set; }
    public DateOnly? ValidTo { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public ProductFamily? Family { get; private set; }
    public ICollection<ProductPlan> Plans { get; private set; } = [];
    public ICollection<ProductRequirement> Requirements { get; private set; } = [];

    private Product() { }

    public static Product Create(
        Guid tenantId, Guid entityId, Guid familyId,
        string name, string code, string? description,
        ProductStatus status, DateOnly validFrom, DateOnly? validTo,
        Guid userId)
    {
        return new Product
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            EntityId = entityId,
            FamilyId = familyId,
            Name = name,
            Code = code,
            Description = description,
            Status = status,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };
    }

    public void Update(string name, string code, string? description, ProductStatus status, DateOnly validFrom, DateOnly? validTo, Guid userId)
    {
        Name = name;
        Code = code;
        Description = description;
        Status = status;
        ValidFrom = validFrom;
        ValidTo = validTo;
        Version++;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void Deprecate(Guid userId)
    {
        Status = ProductStatus.Deprecated;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
}

using Shared.Contract.Enums;

namespace SA.Catalog.Domain.Entities;

public sealed class CommissionPlan
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CommissionValueType Type { get; private set; }
    public decimal Value { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CommissionPlan() { }

    public static CommissionPlan Create(Guid tenantId, string code, string description, CommissionValueType type, decimal value, decimal? maxAmount)
    {
        return new CommissionPlan
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Code = code,
            Description = description,
            Type = type,
            Value = value,
            MaxAmount = maxAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string code, string description, CommissionValueType type, decimal value, decimal? maxAmount)
    {
        Code = code;
        Description = description;
        Type = type;
        Value = value;
        MaxAmount = maxAmount;
        UpdatedAt = DateTime.UtcNow;
    }
}

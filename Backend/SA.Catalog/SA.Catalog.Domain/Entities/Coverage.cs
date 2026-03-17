namespace SA.Catalog.Domain.Entities;

public sealed class Coverage
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string CoverageType { get; private set; } = string.Empty;
    public decimal SumInsured { get; private set; }
    public decimal? Premium { get; private set; }
    public int? GracePeriodDays { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Coverage() { }

    public static Coverage Create(Guid planId, Guid tenantId, string name, string coverageType, decimal sumInsured, decimal? premium, int? gracePeriodDays)
    {
        return new Coverage
        {
            Id = Guid.CreateVersion7(),
            PlanId = planId,
            TenantId = tenantId,
            Name = name,
            CoverageType = coverageType,
            SumInsured = sumInsured,
            Premium = premium,
            GracePeriodDays = gracePeriodDays,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(decimal sumInsured, decimal? premium, int? gracePeriodDays)
    {
        SumInsured = sumInsured;
        Premium = premium;
        GracePeriodDays = gracePeriodDays;
        UpdatedAt = DateTime.UtcNow;
    }
}

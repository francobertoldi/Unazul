namespace SA.Catalog.Domain.Entities;

public sealed class PlanInsuranceAttributes
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public decimal Premium { get; private set; }
    public decimal SumInsured { get; private set; }
    public int? GracePeriodDays { get; private set; }
    public string CoverageType { get; private set; } = string.Empty;

    private PlanInsuranceAttributes() { }

    public static PlanInsuranceAttributes Create(Guid planId, decimal premium, decimal sumInsured, int? gracePeriodDays, string coverageType)
    {
        return new PlanInsuranceAttributes
        {
            Id = Guid.CreateVersion7(),
            PlanId = planId,
            Premium = premium,
            SumInsured = sumInsured,
            GracePeriodDays = gracePeriodDays,
            CoverageType = coverageType
        };
    }

    public void Update(decimal premium, decimal sumInsured, int? gracePeriodDays, string coverageType)
    {
        Premium = premium;
        SumInsured = sumInsured;
        GracePeriodDays = gracePeriodDays;
        CoverageType = coverageType;
    }
}

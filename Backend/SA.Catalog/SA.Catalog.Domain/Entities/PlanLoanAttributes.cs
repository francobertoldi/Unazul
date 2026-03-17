using Shared.Contract.Enums;

namespace SA.Catalog.Domain.Entities;

public sealed class PlanLoanAttributes
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public AmortizationType AmortizationType { get; private set; }
    public decimal AnnualEffectiveRate { get; private set; }
    public decimal? CftRate { get; private set; }
    public decimal? AdminFees { get; private set; }

    private PlanLoanAttributes() { }

    public static PlanLoanAttributes Create(Guid planId, AmortizationType amortizationType, decimal annualEffectiveRate, decimal? cftRate, decimal? adminFees)
    {
        return new PlanLoanAttributes
        {
            Id = Guid.CreateVersion7(),
            PlanId = planId,
            AmortizationType = amortizationType,
            AnnualEffectiveRate = annualEffectiveRate,
            CftRate = cftRate,
            AdminFees = adminFees
        };
    }

    public void Update(AmortizationType amortizationType, decimal annualEffectiveRate, decimal? cftRate, decimal? adminFees)
    {
        AmortizationType = amortizationType;
        AnnualEffectiveRate = annualEffectiveRate;
        CftRate = cftRate;
        AdminFees = adminFees;
    }
}

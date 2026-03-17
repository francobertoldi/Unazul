using Shared.Contract.Enums;

namespace SA.Catalog.Domain.Entities;

public sealed class PlanInvestmentAttributes
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public decimal MinimumAmount { get; private set; }
    public decimal? ExpectedReturn { get; private set; }
    public int? TermDays { get; private set; }
    public RiskLevel RiskLevel { get; private set; }

    private PlanInvestmentAttributes() { }

    public static PlanInvestmentAttributes Create(Guid planId, decimal minimumAmount, decimal? expectedReturn, int? termDays, RiskLevel riskLevel)
    {
        return new PlanInvestmentAttributes
        {
            Id = Guid.CreateVersion7(),
            PlanId = planId,
            MinimumAmount = minimumAmount,
            ExpectedReturn = expectedReturn,
            TermDays = termDays,
            RiskLevel = riskLevel
        };
    }

    public void Update(decimal minimumAmount, decimal? expectedReturn, int? termDays, RiskLevel riskLevel)
    {
        MinimumAmount = minimumAmount;
        ExpectedReturn = expectedReturn;
        TermDays = termDays;
        RiskLevel = riskLevel;
    }
}

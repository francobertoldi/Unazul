using Shared.Contract.Enums;

namespace SA.Catalog.Domain.Entities;

public sealed class PlanCardAttributes
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public decimal CreditLimit { get; private set; }
    public decimal AnnualFee { get; private set; }
    public decimal? InterestRate { get; private set; }
    public CardNetwork Network { get; private set; }
    public string Level { get; private set; } = string.Empty;

    private PlanCardAttributes() { }

    public static PlanCardAttributes Create(Guid planId, decimal creditLimit, decimal annualFee, decimal? interestRate, CardNetwork network, string level)
    {
        return new PlanCardAttributes
        {
            Id = Guid.CreateVersion7(),
            PlanId = planId,
            CreditLimit = creditLimit,
            AnnualFee = annualFee,
            InterestRate = interestRate,
            Network = network,
            Level = level
        };
    }

    public void Update(decimal creditLimit, decimal annualFee, decimal? interestRate, CardNetwork network, string level)
    {
        CreditLimit = creditLimit;
        AnnualFee = annualFee;
        InterestRate = interestRate;
        Network = network;
        Level = level;
    }
}

namespace SA.Catalog.Domain.Entities;

public sealed class PlanAccountAttributes
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public decimal MaintenanceFee { get; private set; }
    public decimal? MinimumBalance { get; private set; }
    public decimal? InterestRate { get; private set; }
    public string AccountType { get; private set; } = string.Empty;

    private PlanAccountAttributes() { }

    public static PlanAccountAttributes Create(Guid planId, decimal maintenanceFee, decimal? minimumBalance, decimal? interestRate, string accountType)
    {
        return new PlanAccountAttributes
        {
            Id = Guid.CreateVersion7(),
            PlanId = planId,
            MaintenanceFee = maintenanceFee,
            MinimumBalance = minimumBalance,
            InterestRate = interestRate,
            AccountType = accountType
        };
    }

    public void Update(decimal maintenanceFee, decimal? minimumBalance, decimal? interestRate, string accountType)
    {
        MaintenanceFee = maintenanceFee;
        MinimumBalance = minimumBalance;
        InterestRate = interestRate;
        AccountType = accountType;
    }
}

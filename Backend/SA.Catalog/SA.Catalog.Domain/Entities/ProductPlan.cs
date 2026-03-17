namespace SA.Catalog.Domain.Entities;

public sealed class ProductPlan
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int? Installments { get; private set; }
    public Guid? CommissionPlanId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public CommissionPlan? CommissionPlan { get; private set; }
    public PlanLoanAttributes? LoanAttributes { get; private set; }
    public PlanInsuranceAttributes? InsuranceAttributes { get; private set; }
    public PlanAccountAttributes? AccountAttributes { get; private set; }
    public PlanCardAttributes? CardAttributes { get; private set; }
    public PlanInvestmentAttributes? InvestmentAttributes { get; private set; }
    public ICollection<Coverage> Coverages { get; private set; } = [];

    private ProductPlan() { }

    public static ProductPlan Create(
        Guid productId, Guid tenantId,
        string name, string code, decimal price, string currency,
        int? installments, Guid? commissionPlanId)
    {
        return new ProductPlan
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            TenantId = tenantId,
            Name = name,
            Code = code,
            Price = price,
            Currency = currency,
            Installments = installments,
            CommissionPlanId = commissionPlanId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string code, decimal price, string currency, int? installments, Guid? commissionPlanId)
    {
        Name = name;
        Code = code;
        Price = price;
        Currency = currency;
        Installments = installments;
        CommissionPlanId = commissionPlanId;
        UpdatedAt = DateTime.UtcNow;
    }
}

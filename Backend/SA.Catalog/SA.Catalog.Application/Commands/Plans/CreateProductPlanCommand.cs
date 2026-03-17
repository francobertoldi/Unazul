using Mediator;

namespace SA.Catalog.Application.Commands.Plans;

public readonly record struct CreateProductPlanCommand(
    Guid TenantId, Guid ProductId,
    string Name, string Code, decimal Price, string Currency,
    int? Installments, Guid? CommissionPlanId,
    LoanAttributesInput? LoanAttributes,
    InsuranceAttributesInput? InsuranceAttributes,
    AccountAttributesInput? AccountAttributes,
    CardAttributesInput? CardAttributes,
    InvestmentAttributesInput? InvestmentAttributes,
    CoverageInput[]? Coverages,
    Guid UserId) : ICommand<Guid>;

public sealed record LoanAttributesInput(string AmortizationType, decimal AnnualEffectiveRate, decimal? CftRate, decimal? AdminFees);
public sealed record InsuranceAttributesInput(decimal Premium, decimal SumInsured, int? GracePeriodDays, string CoverageType);
public sealed record AccountAttributesInput(decimal MaintenanceFee, decimal? MinimumBalance, decimal? InterestRate, string AccountType);
public sealed record CardAttributesInput(decimal CreditLimit, decimal AnnualFee, decimal? InterestRate, string Network, string Level);
public sealed record InvestmentAttributesInput(decimal MinimumAmount, decimal? ExpectedReturn, int? TermDays, string RiskLevel);
public sealed record CoverageInput(string Name, string CoverageType, decimal SumInsured, decimal? Premium, int? GracePeriodDays);

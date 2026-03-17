using SA.Catalog.Application.Commands.Plans;

namespace SA.Catalog.Api.ViewModels.Plans;

public sealed record CreateProductPlanRequest(
    string Name,
    string Code,
    decimal Price,
    string Currency,
    int? Installments,
    Guid? CommissionPlanId,
    LoanAttributesInput? LoanAttributes,
    InsuranceAttributesInput? InsuranceAttributes,
    AccountAttributesInput? AccountAttributes,
    CardAttributesInput? CardAttributes,
    InvestmentAttributesInput? InvestmentAttributes,
    CreatePlanCoverageRequest[]? Coverages);

public sealed record CreatePlanCoverageRequest(
    string Name,
    string CoverageType,
    decimal SumInsured,
    decimal? Premium,
    int? GracePeriodDays);

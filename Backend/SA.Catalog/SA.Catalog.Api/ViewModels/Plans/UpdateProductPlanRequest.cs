using SA.Catalog.Application.Commands.Plans;

namespace SA.Catalog.Api.ViewModels.Plans;

public sealed record UpdateProductPlanRequest(
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
    CoverageInput[]? Coverages);

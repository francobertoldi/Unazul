using Mediator;

namespace SA.Catalog.Application.Commands.Plans;

public readonly record struct UpdateProductPlanCommand(
    Guid TenantId, Guid Id, Guid ProductId,
    string Name, string Code, decimal Price, string Currency,
    int? Installments, Guid? CommissionPlanId,
    LoanAttributesInput? LoanAttributes,
    InsuranceAttributesInput? InsuranceAttributes,
    AccountAttributesInput? AccountAttributes,
    CardAttributesInput? CardAttributes,
    InvestmentAttributesInput? InvestmentAttributes,
    CoverageInput[]? Coverages,
    Guid UserId) : ICommand;

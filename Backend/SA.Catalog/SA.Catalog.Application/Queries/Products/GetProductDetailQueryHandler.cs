using Mediator;
using SA.Catalog.Application.Dtos;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.Application.Queries.Products;

public sealed class GetProductDetailQueryHandler(
    IProductRepository productRepository) : IQueryHandler<GetProductDetailQuery, ProductDetailDto>
{
    public async ValueTask<ProductDetailDto> Handle(GetProductDetailQuery query, CancellationToken ct)
    {
        var product = await productRepository.GetByIdWithDetailsAsync(query.Id, ct)
            ?? throw new InvalidOperationException("CAT_PRODUCT_NOT_FOUND");

        var familyCode = product.Family?.Code ?? string.Empty;
        var familyDescription = product.Family?.Description ?? string.Empty;
        var category = ProductCategory.GetCategoryFromCode(familyCode) ?? "unknown";

        var plans = product.Plans
            .Select(p => MapPlanDto(p, category))
            .ToList();

        var requirements = product.Requirements
            .Select(r => new ProductRequirementDto(
                r.Id, r.Name, r.Type, r.IsMandatory, r.Description))
            .ToList();

        return new ProductDetailDto(
            product.Id, product.Name, product.Code, product.Description,
            product.Status.ToString().ToLowerInvariant(),
            product.EntityId, product.FamilyId,
            familyCode, familyDescription, category,
            product.ValidFrom, product.ValidTo, product.Version,
            product.CreatedAt, product.UpdatedAt,
            plans, requirements);
    }

    private static ProductPlanDto MapPlanDto(ProductPlan plan, string category)
    {
        object? categoryAttributes = category switch
        {
            ProductCategory.Loan when plan.LoanAttributes is not null =>
                new LoanAttributesDto(
                    plan.LoanAttributes.AmortizationType.ToString().ToLowerInvariant(),
                    plan.LoanAttributes.AnnualEffectiveRate,
                    plan.LoanAttributes.CftRate,
                    plan.LoanAttributes.AdminFees),

            ProductCategory.Insurance when plan.InsuranceAttributes is not null =>
                new InsuranceAttributesDto(
                    plan.InsuranceAttributes.Premium,
                    plan.InsuranceAttributes.SumInsured,
                    plan.InsuranceAttributes.GracePeriodDays,
                    plan.InsuranceAttributes.CoverageType),

            ProductCategory.Account when plan.AccountAttributes is not null =>
                new AccountAttributesDto(
                    plan.AccountAttributes.MaintenanceFee,
                    plan.AccountAttributes.MinimumBalance,
                    plan.AccountAttributes.InterestRate,
                    plan.AccountAttributes.AccountType),

            ProductCategory.Card when plan.CardAttributes is not null =>
                new CardAttributesDto(
                    plan.CardAttributes.CreditLimit,
                    plan.CardAttributes.AnnualFee,
                    plan.CardAttributes.InterestRate,
                    plan.CardAttributes.Network.ToString().ToLowerInvariant(),
                    plan.CardAttributes.Level),

            ProductCategory.Investment when plan.InvestmentAttributes is not null =>
                new InvestmentAttributesDto(
                    plan.InvestmentAttributes.MinimumAmount,
                    plan.InvestmentAttributes.ExpectedReturn,
                    plan.InvestmentAttributes.TermDays,
                    plan.InvestmentAttributes.RiskLevel.ToString().ToLowerInvariant()),

            _ => null
        };

        IReadOnlyList<CoverageDto>? coverages = category == ProductCategory.Insurance
            ? plan.Coverages
                .Select(c => new CoverageDto(
                    c.Id, c.Name, c.CoverageType,
                    c.SumInsured, c.Premium, c.GracePeriodDays))
                .ToList()
            : null;

        return new ProductPlanDto(
            plan.Id, plan.Name, plan.Code,
            plan.Price, plan.Currency,
            plan.Installments, plan.CommissionPlanId,
            plan.CommissionPlan?.Code,
            plan.CommissionPlan?.Description,
            categoryAttributes, coverages);
    }
}

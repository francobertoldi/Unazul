using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Plans;

public sealed class CreateProductPlanCommandHandler(
    IProductRepository productRepository,
    IProductFamilyRepository familyRepository,
    IProductPlanRepository planRepository,
    ICommissionPlanRepository commissionPlanRepository,
    IPlanLoanAttributesRepository loanAttributesRepository,
    IPlanInsuranceAttributesRepository insuranceAttributesRepository,
    IPlanAccountAttributesRepository accountAttributesRepository,
    IPlanCardAttributesRepository cardAttributesRepository,
    IPlanInvestmentAttributesRepository investmentAttributesRepository,
    ICoverageRepository coverageRepository) : ICommandHandler<CreateProductPlanCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProductPlanCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Code)
            || string.IsNullOrWhiteSpace(command.Currency))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        if (command.Price < 0)
            throw new ValidationException("CAT_INVALID_PRICE", "El precio no puede ser negativo.");

        var product = await productRepository.GetByIdAsync(command.ProductId, ct)
            ?? throw new NotFoundException("CAT_PRODUCT_NOT_FOUND", "Producto no encontrado.");

        if (product.Status == ProductStatus.Deprecated)
            throw new ConflictException("CAT_PRODUCT_DEPRECATED", "El producto esta deprecado y no se puede modificar.");

        var family = await familyRepository.GetByIdAsync(product.FamilyId, ct)
            ?? throw new NotFoundException("CAT_FAMILY_NOT_FOUND", "Familia no encontrada.");

        var category = ProductCategory.GetCategoryFromCode(family.Code);
        ValidateCategoryAttributes(category, command);

        if (command.CommissionPlanId.HasValue)
        {
            var commissionPlan = await commissionPlanRepository.GetByIdAsync(command.CommissionPlanId.Value, ct);
            if (commissionPlan is null)
                throw new NotFoundException("CAT_COMMISSION_PLAN_NOT_FOUND", "Plan de comision no encontrado.");
        }

        var plan = ProductPlan.Create(
            command.ProductId, command.TenantId,
            command.Name, command.Code, command.Price, command.Currency,
            command.Installments, command.CommissionPlanId);

        await planRepository.AddAsync(plan, ct);

        await CreateCategoryAttributes(plan.Id, category, command, ct);

        if (category == ProductCategory.Insurance && command.Coverages is { Length: > 0 })
        {
            var coverages = command.Coverages
                .Select(c => Coverage.Create(plan.Id, command.TenantId,
                    c.Name, c.CoverageType, c.SumInsured, c.Premium, c.GracePeriodDays))
                .ToList();

            await coverageRepository.AddRangeAsync(coverages, ct);
        }

        await planRepository.SaveChangesAsync(ct);

        return plan.Id;
    }

    private static void ValidateCategoryAttributes(string? category, CreateProductPlanCommand command)
    {
        switch (category)
        {
            case ProductCategory.Loan:
                if (command.LoanAttributes is null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                if (command.InsuranceAttributes is not null || command.AccountAttributes is not null
                    || command.CardAttributes is not null || command.InvestmentAttributes is not null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                if (!Enum.TryParse<AmortizationType>(command.LoanAttributes.AmortizationType, true, out _))
                    throw new ValidationException("CAT_INVALID_AMORTIZATION_TYPE", "Tipo de amortizacion no valido.");
                break;

            case ProductCategory.Insurance:
                if (command.InsuranceAttributes is null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                if (command.LoanAttributes is not null || command.AccountAttributes is not null
                    || command.CardAttributes is not null || command.InvestmentAttributes is not null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                break;

            case ProductCategory.Account:
                if (command.AccountAttributes is null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                if (command.LoanAttributes is not null || command.InsuranceAttributes is not null
                    || command.CardAttributes is not null || command.InvestmentAttributes is not null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                break;

            case ProductCategory.Card:
                if (command.CardAttributes is null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                if (command.LoanAttributes is not null || command.InsuranceAttributes is not null
                    || command.AccountAttributes is not null || command.InvestmentAttributes is not null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                if (!Enum.TryParse<CardNetwork>(command.CardAttributes.Network, true, out _))
                    throw new ValidationException("CAT_INVALID_CARD_NETWORK", "Red de tarjeta no valida.");
                break;

            case ProductCategory.Investment:
                if (command.InvestmentAttributes is null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                if (command.LoanAttributes is not null || command.InsuranceAttributes is not null
                    || command.AccountAttributes is not null || command.CardAttributes is not null)
                    throw new ValidationException("CAT_CATEGORY_MISMATCH", "Los atributos de categoria no coinciden con el tipo de producto.");
                if (!Enum.TryParse<RiskLevel>(command.InvestmentAttributes.RiskLevel, true, out _))
                    throw new ValidationException("CAT_INVALID_RISK_LEVEL", "Nivel de riesgo no valido.");
                break;
        }
    }

    private async Task CreateCategoryAttributes(Guid planId, string? category,
        CreateProductPlanCommand command, CancellationToken ct)
    {
        switch (category)
        {
            case ProductCategory.Loan when command.LoanAttributes is not null:
                var loanAttrs = PlanLoanAttributes.Create(planId,
                    Enum.Parse<AmortizationType>(command.LoanAttributes.AmortizationType, true),
                    command.LoanAttributes.AnnualEffectiveRate,
                    command.LoanAttributes.CftRate,
                    command.LoanAttributes.AdminFees);
                await loanAttributesRepository.AddAsync(loanAttrs, ct);
                break;

            case ProductCategory.Insurance when command.InsuranceAttributes is not null:
                var insAttrs = PlanInsuranceAttributes.Create(planId,
                    command.InsuranceAttributes.Premium,
                    command.InsuranceAttributes.SumInsured,
                    command.InsuranceAttributes.GracePeriodDays,
                    command.InsuranceAttributes.CoverageType);
                await insuranceAttributesRepository.AddAsync(insAttrs, ct);
                break;

            case ProductCategory.Account when command.AccountAttributes is not null:
                var accAttrs = PlanAccountAttributes.Create(planId,
                    command.AccountAttributes.MaintenanceFee,
                    command.AccountAttributes.MinimumBalance,
                    command.AccountAttributes.InterestRate,
                    command.AccountAttributes.AccountType);
                await accountAttributesRepository.AddAsync(accAttrs, ct);
                break;

            case ProductCategory.Card when command.CardAttributes is not null:
                var cardAttrs = PlanCardAttributes.Create(planId,
                    command.CardAttributes.CreditLimit,
                    command.CardAttributes.AnnualFee,
                    command.CardAttributes.InterestRate,
                    Enum.Parse<CardNetwork>(command.CardAttributes.Network, true),
                    command.CardAttributes.Level);
                await cardAttributesRepository.AddAsync(cardAttrs, ct);
                break;

            case ProductCategory.Investment when command.InvestmentAttributes is not null:
                var invAttrs = PlanInvestmentAttributes.Create(planId,
                    command.InvestmentAttributes.MinimumAmount,
                    command.InvestmentAttributes.ExpectedReturn,
                    command.InvestmentAttributes.TermDays,
                    Enum.Parse<RiskLevel>(command.InvestmentAttributes.RiskLevel, true));
                await investmentAttributesRepository.AddAsync(invAttrs, ct);
                break;
        }
    }
}

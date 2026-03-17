using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Plans;

public sealed class UpdateProductPlanCommandHandler(
    IProductRepository productRepository,
    IProductFamilyRepository familyRepository,
    IProductPlanRepository planRepository,
    ICommissionPlanRepository commissionPlanRepository,
    IPlanLoanAttributesRepository loanAttributesRepository,
    IPlanInsuranceAttributesRepository insuranceAttributesRepository,
    IPlanAccountAttributesRepository accountAttributesRepository,
    IPlanCardAttributesRepository cardAttributesRepository,
    IPlanInvestmentAttributesRepository investmentAttributesRepository,
    ICoverageRepository coverageRepository) : ICommandHandler<UpdateProductPlanCommand>
{
    public async ValueTask<Unit> Handle(UpdateProductPlanCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Code)
            || string.IsNullOrWhiteSpace(command.Currency))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        if (command.Price < 0)
            throw new ValidationException("CAT_INVALID_PRICE", "El precio no puede ser negativo.");

        var plan = await planRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_PLAN_NOT_FOUND", "Plan no encontrado.");

        var product = await productRepository.GetByIdAsync(command.ProductId, ct)
            ?? throw new NotFoundException("CAT_PRODUCT_NOT_FOUND", "Producto no encontrado.");

        if (product.Status == ProductStatus.Deprecated)
            throw new ConflictException("CAT_PRODUCT_DEPRECATED", "El producto esta deprecado y no se puede modificar.");

        var family = await familyRepository.GetByIdAsync(product.FamilyId, ct)
            ?? throw new NotFoundException("CAT_FAMILY_NOT_FOUND", "Familia no encontrada.");

        var category = ProductCategory.GetCategoryFromCode(family.Code);

        if (command.CommissionPlanId.HasValue)
        {
            var commissionPlan = await commissionPlanRepository.GetByIdAsync(command.CommissionPlanId.Value, ct);
            if (commissionPlan is null)
                throw new NotFoundException("CAT_COMMISSION_PLAN_NOT_FOUND", "Plan de comision no encontrado.");
        }

        plan.Update(command.Name, command.Code, command.Price, command.Currency,
            command.Installments, command.CommissionPlanId);
        planRepository.Update(plan);

        await UpdateCategoryAttributes(plan.Id, category, command, ct);

        if (category == ProductCategory.Insurance)
        {
            await coverageRepository.DeleteByPlanIdAsync(plan.Id, ct);

            if (command.Coverages is { Length: > 0 })
            {
                var coverages = command.Coverages
                    .Select(c => Coverage.Create(plan.Id, command.TenantId,
                        c.Name, c.CoverageType, c.SumInsured, c.Premium, c.GracePeriodDays))
                    .ToList();

                await coverageRepository.AddRangeAsync(coverages, ct);
            }
        }

        await planRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }

    private async Task UpdateCategoryAttributes(Guid planId, string? category,
        UpdateProductPlanCommand command, CancellationToken ct)
    {
        switch (category)
        {
            case ProductCategory.Loan when command.LoanAttributes is not null:
                var existingLoan = await loanAttributesRepository.GetByPlanIdAsync(planId, ct);
                if (existingLoan is not null)
                {
                    existingLoan.Update(
                        Enum.Parse<AmortizationType>(command.LoanAttributes.AmortizationType, true),
                        command.LoanAttributes.AnnualEffectiveRate,
                        command.LoanAttributes.CftRate,
                        command.LoanAttributes.AdminFees);
                    loanAttributesRepository.Update(existingLoan);
                }
                else
                {
                    var loanAttrs = PlanLoanAttributes.Create(planId,
                        Enum.Parse<AmortizationType>(command.LoanAttributes.AmortizationType, true),
                        command.LoanAttributes.AnnualEffectiveRate,
                        command.LoanAttributes.CftRate,
                        command.LoanAttributes.AdminFees);
                    await loanAttributesRepository.AddAsync(loanAttrs, ct);
                }
                break;

            case ProductCategory.Insurance when command.InsuranceAttributes is not null:
                var existingIns = await insuranceAttributesRepository.GetByPlanIdAsync(planId, ct);
                if (existingIns is not null)
                {
                    existingIns.Update(
                        command.InsuranceAttributes.Premium,
                        command.InsuranceAttributes.SumInsured,
                        command.InsuranceAttributes.GracePeriodDays,
                        command.InsuranceAttributes.CoverageType);
                    insuranceAttributesRepository.Update(existingIns);
                }
                else
                {
                    var insAttrs = PlanInsuranceAttributes.Create(planId,
                        command.InsuranceAttributes.Premium,
                        command.InsuranceAttributes.SumInsured,
                        command.InsuranceAttributes.GracePeriodDays,
                        command.InsuranceAttributes.CoverageType);
                    await insuranceAttributesRepository.AddAsync(insAttrs, ct);
                }
                break;

            case ProductCategory.Account when command.AccountAttributes is not null:
                var existingAcc = await accountAttributesRepository.GetByPlanIdAsync(planId, ct);
                if (existingAcc is not null)
                {
                    existingAcc.Update(
                        command.AccountAttributes.MaintenanceFee,
                        command.AccountAttributes.MinimumBalance,
                        command.AccountAttributes.InterestRate,
                        command.AccountAttributes.AccountType);
                    accountAttributesRepository.Update(existingAcc);
                }
                else
                {
                    var accAttrs = PlanAccountAttributes.Create(planId,
                        command.AccountAttributes.MaintenanceFee,
                        command.AccountAttributes.MinimumBalance,
                        command.AccountAttributes.InterestRate,
                        command.AccountAttributes.AccountType);
                    await accountAttributesRepository.AddAsync(accAttrs, ct);
                }
                break;

            case ProductCategory.Card when command.CardAttributes is not null:
                var existingCard = await cardAttributesRepository.GetByPlanIdAsync(planId, ct);
                if (existingCard is not null)
                {
                    existingCard.Update(
                        command.CardAttributes.CreditLimit,
                        command.CardAttributes.AnnualFee,
                        command.CardAttributes.InterestRate,
                        Enum.Parse<CardNetwork>(command.CardAttributes.Network, true),
                        command.CardAttributes.Level);
                    cardAttributesRepository.Update(existingCard);
                }
                else
                {
                    var cardAttrs = PlanCardAttributes.Create(planId,
                        command.CardAttributes.CreditLimit,
                        command.CardAttributes.AnnualFee,
                        command.CardAttributes.InterestRate,
                        Enum.Parse<CardNetwork>(command.CardAttributes.Network, true),
                        command.CardAttributes.Level);
                    await cardAttributesRepository.AddAsync(cardAttrs, ct);
                }
                break;

            case ProductCategory.Investment when command.InvestmentAttributes is not null:
                var existingInv = await investmentAttributesRepository.GetByPlanIdAsync(planId, ct);
                if (existingInv is not null)
                {
                    existingInv.Update(
                        command.InvestmentAttributes.MinimumAmount,
                        command.InvestmentAttributes.ExpectedReturn,
                        command.InvestmentAttributes.TermDays,
                        Enum.Parse<RiskLevel>(command.InvestmentAttributes.RiskLevel, true));
                    investmentAttributesRepository.Update(existingInv);
                }
                else
                {
                    var invAttrs = PlanInvestmentAttributes.Create(planId,
                        command.InvestmentAttributes.MinimumAmount,
                        command.InvestmentAttributes.ExpectedReturn,
                        command.InvestmentAttributes.TermDays,
                        Enum.Parse<RiskLevel>(command.InvestmentAttributes.RiskLevel, true));
                    await investmentAttributesRepository.AddAsync(invAttrs, ct);
                }
                break;
        }
    }
}

using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Plans;

public sealed class DeleteProductPlanCommandHandler(
    IProductPlanRepository planRepository,
    ICoverageRepository coverageRepository,
    IPlanLoanAttributesRepository loanAttributesRepository,
    IPlanInsuranceAttributesRepository insuranceAttributesRepository,
    IPlanAccountAttributesRepository accountAttributesRepository,
    IPlanCardAttributesRepository cardAttributesRepository,
    IPlanInvestmentAttributesRepository investmentAttributesRepository) : ICommandHandler<DeleteProductPlanCommand>
{
    public async ValueTask<Unit> Handle(DeleteProductPlanCommand command, CancellationToken ct)
    {
        var plan = await planRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_PLAN_NOT_FOUND", "Plan no encontrado.");

        await coverageRepository.DeleteByPlanIdAsync(plan.Id, ct);
        await loanAttributesRepository.DeleteByPlanIdAsync(plan.Id, ct);
        await insuranceAttributesRepository.DeleteByPlanIdAsync(plan.Id, ct);
        await accountAttributesRepository.DeleteByPlanIdAsync(plan.Id, ct);
        await cardAttributesRepository.DeleteByPlanIdAsync(plan.Id, ct);
        await investmentAttributesRepository.DeleteByPlanIdAsync(plan.Id, ct);

        planRepository.Delete(plan);
        await planRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

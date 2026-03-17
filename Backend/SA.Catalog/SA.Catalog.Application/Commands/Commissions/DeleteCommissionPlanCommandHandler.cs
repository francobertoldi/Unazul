using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Commissions;

public sealed class DeleteCommissionPlanCommandHandler(
    ICommissionPlanRepository commissionPlanRepository) : ICommandHandler<DeleteCommissionPlanCommand>
{
    public async ValueTask<Unit> Handle(DeleteCommissionPlanCommand command, CancellationToken ct)
    {
        var commissionPlan = await commissionPlanRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_COMMISSION_PLAN_NOT_FOUND", "Plan de comision no encontrado.");

        var assignedCount = await commissionPlanRepository.CountAssignedPlansAsync(command.Id, ct);
        if (assignedCount > 0)
            throw new ConflictException("CAT_COMMISSION_IN_USE", "El plan de comision esta en uso y no se puede eliminar.");

        commissionPlanRepository.Delete(commissionPlan);
        await commissionPlanRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

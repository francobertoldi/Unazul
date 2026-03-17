using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Commissions;

public sealed class UpdateCommissionPlanCommandHandler(
    ICommissionPlanRepository commissionPlanRepository) : ICommandHandler<UpdateCommissionPlanCommand>
{
    public async ValueTask<Unit> Handle(UpdateCommissionPlanCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Description))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        if (!Enum.TryParse<CommissionValueType>(command.Type, true, out var type))
            throw new ValidationException("CAT_INVALID_COMMISSION_TYPE", "Tipo de comision no valido.");

        if (command.Value <= 0)
            throw new ValidationException("CAT_INVALID_COMMISSION_VALUE", "El valor de la comision debe ser mayor a cero.");

        var commissionPlan = await commissionPlanRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_COMMISSION_PLAN_NOT_FOUND", "Plan de comision no encontrado.");

        var codeExists = await commissionPlanRepository.ExistsByCodeExcludingAsync(
            command.TenantId, command.Code, command.Id, ct);
        if (codeExists)
            throw new ConflictException("CAT_DUPLICATE_COMMISSION_CODE", "Ya existe un plan de comision con ese codigo.");

        commissionPlan.Update(command.Code, command.Description, type, command.Value, command.MaxAmount);
        commissionPlanRepository.Update(commissionPlan);
        await commissionPlanRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

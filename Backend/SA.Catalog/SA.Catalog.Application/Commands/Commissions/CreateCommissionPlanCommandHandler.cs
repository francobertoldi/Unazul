using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Commissions;

public sealed class CreateCommissionPlanCommandHandler(
    ICommissionPlanRepository commissionPlanRepository) : ICommandHandler<CreateCommissionPlanCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateCommissionPlanCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Description))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        if (!Enum.TryParse<CommissionValueType>(command.Type, true, out var type))
            throw new ValidationException("CAT_INVALID_COMMISSION_TYPE", "Tipo de comision no valido.");

        if (command.Value <= 0)
            throw new ValidationException("CAT_INVALID_COMMISSION_VALUE", "El valor de la comision debe ser mayor a cero.");

        var exists = await commissionPlanRepository.ExistsByCodeAsync(command.TenantId, command.Code, ct);
        if (exists)
            throw new ConflictException("CAT_DUPLICATE_COMMISSION_CODE", "Ya existe un plan de comision con ese codigo.");

        var commissionPlan = CommissionPlan.Create(
            command.TenantId, command.Code, command.Description,
            type, command.Value, command.MaxAmount);

        await commissionPlanRepository.AddAsync(commissionPlan, ct);
        await commissionPlanRepository.SaveChangesAsync(ct);

        return commissionPlan.Id;
    }
}

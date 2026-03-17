using Mediator;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Parameters;

public sealed class DeleteParameterCommandHandler(
    IParameterRepository parameterRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<DeleteParameterCommand>
{
    public async ValueTask<Unit> Handle(DeleteParameterCommand command, CancellationToken ct)
    {
        var parameter = await parameterRepository.GetByIdAsync(command.Id, ct);
        if (parameter is null)
        {
            throw new NotFoundException("CFG_PARAMETER_NOT_FOUND", "Parametro no encontrado.");
        }

        var tenantId = parameter.TenantId;
        var key = parameter.Key;
        var oldValue = parameter.Value;
        var updatedBy = parameter.UpdatedBy;

        await parameterRepository.DeleteAsync(parameter, ct);
        await parameterRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new ParameterUpdatedEvent(
            TenantId: tenantId,
            ParameterId: command.Id,
            ParameterCode: key,
            OldValue: oldValue,
            NewValue: null,
            UpdatedBy: updatedBy,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return Unit.Value;
    }
}

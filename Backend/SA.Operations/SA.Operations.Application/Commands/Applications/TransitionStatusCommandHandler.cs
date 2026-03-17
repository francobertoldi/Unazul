using Mediator;
using SA.Operations.Application.Events;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Applications;

public sealed class TransitionStatusCommandHandler(
    IApplicationRepository applicationRepository,
    IBeneficiaryRepository beneficiaryRepository,
    ITraceEventRepository traceEventRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<TransitionStatusCommand, TransitionStatusResult>
{
    public async ValueTask<TransitionStatusResult> Handle(TransitionStatusCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<ApplicationStatus>(command.NewStatus, true, out var newStatus))
            throw new ValidationException("OPS_INVALID_STATUS", "Estado invalido.");

        // Settled is NOT allowed via this command — only via settlement process
        if (newStatus == ApplicationStatus.Settled)
            throw new ValidationException("OPS_SETTLED_VIA_SETTLEMENT_ONLY", "El estado Liquidado solo se asigna mediante el proceso de liquidacion.");

        var app = await applicationRepository.GetByIdAsync(command.ApplicationId, ct);
        if (app is null || app.TenantId != command.TenantId)
            throw new NotFoundException("OPS_APPLICATION_NOT_FOUND", "Solicitud no encontrada.");

        var oldStatus = app.Status;

        // If transitioning from Draft to Pending, validate beneficiary sum = 100%
        if (oldStatus == ApplicationStatus.Draft && newStatus == ApplicationStatus.Pending)
        {
            var beneficiarySum = await beneficiaryRepository.SumPercentageAsync(app.Id, ct);
            if (beneficiarySum != 100m)
                throw new ValidationException("OPS_BENEFICIARY_SUM_NOT_100", "La suma de porcentajes de beneficiarios debe ser 100%.");
        }

        // Optimistic concurrency: attempt transition via repository
        var affected = await applicationRepository.TransitionStatusAsync(
            app.Id, oldStatus, newStatus, command.UserId, ct);

        if (affected == 0)
            throw new ConflictException("OPS_TRANSITION_CONFLICT", "Conflicto de concurrencia al transicionar el estado.");

        // Create trace event
        var traceEvent = TraceEvent.Create(
            app.Id,
            command.TenantId,
            newStatus.ToString(),
            command.Action,
            command.UserId,
            command.UserName);
        await traceEventRepository.AddAsync(traceEvent, ct);

        // Add detail if provided
        if (!string.IsNullOrWhiteSpace(command.Detail))
        {
            var detail = TraceEventDetail.Create(traceEvent.Id, command.TenantId, "detail", command.Detail);
            await traceEventRepository.SaveChangesAsync(ct);
        }

        await traceEventRepository.SaveChangesAsync(ct);

        // Publish event
        await eventPublisher.PublishAsync(new ApplicationStatusChangedEvent(
            app.Id,
            oldStatus.ToString(),
            newStatus.ToString(),
            command.UserId,
            command.TenantId,
            DateTimeOffset.UtcNow,
            Guid.CreateVersion7()), ct);

        return new TransitionStatusResult(app.Id, app.Code, newStatus.ToString());
    }
}

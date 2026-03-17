using Mediator;
using SA.Config.Application.Dtos.Workflows;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Workflows;

public sealed class DeactivateWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<DeactivateWorkflowCommand, WorkflowSummaryDto>
{
    public async ValueTask<WorkflowSummaryDto> Handle(DeactivateWorkflowCommand command, CancellationToken ct)
    {
        var workflow = await workflowRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("WORKFLOW_NOT_FOUND", "Workflow no encontrado.");

        if (!workflow.IsActive)
            throw new ConflictException("WORKFLOW_NOT_ACTIVE", "El workflow no se encuentra activo.");

        workflow.Deactivate(command.UpdatedBy);
        workflowRepository.Update(workflow);
        await workflowRepository.SaveChangesAsync(ct);

        // Publish event
        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: workflow.TenantId,
            UserId: command.UpdatedBy,
            UserName: string.Empty,
            Operation: "WRITE",
            Module: "config",
            Action: "workflow_deactivated",
            Detail: $"Workflow '{workflow.Name}' deactivated",
            IpAddress: null,
            EntityType: "WorkflowDefinition",
            EntityId: workflow.Id,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return new WorkflowSummaryDto(
            workflow.Id,
            workflow.Name,
            workflow.Status.ToString(),
            workflow.Version,
            workflow.CreatedAt);
    }
}

using Mediator;
using SA.Config.Application.Dtos.Workflows;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Workflows;

public sealed class UpdateWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<UpdateWorkflowCommand, WorkflowSummaryDto>
{
    public async ValueTask<WorkflowSummaryDto> Handle(UpdateWorkflowCommand command, CancellationToken ct)
    {
        var workflow = await workflowRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("WORKFLOW_NOT_FOUND", "Workflow no encontrado.");

        // Validate name
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("WORKFLOW_NAME_REQUIRED", "El nombre del workflow es obligatorio.");

        // Validate states not empty
        if (command.States.Length == 0)
            throw new ValidationException("WORKFLOW_STATES_REQUIRED", "El workflow debe tener al menos un estado.");

        // Validate node configs per type (RF-CFG-14)
        WorkflowValidationHelper.ValidateStateConfigs(command.States);

        // Validate transitions
        WorkflowValidationHelper.ValidateTransitions(command.Transitions, command.States.Length);

        // If active: revert to draft (RN-CFG-12)
        workflow.UpdateDraft(command.Name, command.Description, command.UpdatedBy);
        workflowRepository.Update(workflow);

        // Build new states
        var stateEntities = new List<WorkflowState>();
        foreach (var stateInput in command.States)
        {
            if (!Enum.TryParse<FlowNodeType>(stateInput.Type, ignoreCase: true, out var nodeType))
                throw new ValidationException("WORKFLOW_INVALID_NODE_TYPE", $"Tipo de nodo invalido: {stateInput.Type}");

            var state = WorkflowState.Create(
                workflow.Id,
                workflow.TenantId,
                stateInput.Name,
                stateInput.Label,
                nodeType,
                stateInput.PositionX,
                stateInput.PositionY);

            stateEntities.Add(state);
        }

        // Create configs and fields for each state
        for (var i = 0; i < command.States.Length; i++)
        {
            var stateInput = command.States[i];
            var stateEntity = stateEntities[i];

            if (stateInput.Configs is { Length: > 0 })
            {
                foreach (var cfg in stateInput.Configs)
                {
                    stateEntity.Configs.Add(WorkflowStateConfig.Create(
                        stateEntity.Id, workflow.TenantId, cfg.Key, cfg.Value));
                }
            }

            if (stateInput.Fields is { Length: > 0 })
            {
                foreach (var field in stateInput.Fields)
                {
                    stateEntity.Fields.Add(WorkflowStateField.Create(
                        stateEntity.Id, workflow.TenantId, field.FieldName, field.FieldType, field.IsRequired, field.SortOrder));
                }
            }
        }

        // Build new transitions
        var transitionEntities = new List<WorkflowTransition>();
        foreach (var tr in command.Transitions)
        {
            var transition = WorkflowTransition.Create(
                workflow.Id,
                workflow.TenantId,
                stateEntities[tr.FromStateIndex].Id,
                stateEntities[tr.ToStateIndex].Id,
                tr.Label,
                tr.Condition,
                tr.SlaHours);

            transitionEntities.Add(transition);
        }

        // Replace strategy: delete all children, re-insert
        await workflowRepository.ReplaceChildrenAsync(
            workflow.Id, workflow.TenantId, stateEntities, transitionEntities, ct);

        await workflowRepository.SaveChangesAsync(ct);

        // Publish event
        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: workflow.TenantId,
            UserId: command.UpdatedBy,
            UserName: string.Empty,
            Operation: "WRITE",
            Module: "config",
            Action: "workflow_updated",
            Detail: $"Workflow '{command.Name}' updated",
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

using Mediator;
using SA.Config.Application.Dtos.Workflows;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Workflows;

public sealed class ActivateWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IExternalServiceRepository externalServiceRepository,
    INotificationTemplateRepository notificationTemplateRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<ActivateWorkflowCommand, WorkflowSummaryDto>
{
    public async ValueTask<WorkflowSummaryDto> Handle(ActivateWorkflowCommand command, CancellationToken ct)
    {
        var workflow = await workflowRepository.GetByIdFullAsync(command.Id, ct)
            ?? throw new NotFoundException("WORKFLOW_NOT_FOUND", "Workflow no encontrado.");

        if (workflow.IsActive)
            throw new ConflictException("WORKFLOW_ALREADY_ACTIVE", "El workflow ya se encuentra activo.");

        // Collect ALL validation errors
        var errors = new List<string>();

        var states = workflow.States.ToList();
        var transitions = workflow.Transitions.ToList();

        // Exactly 1 start node
        var startNodes = states.Where(s => s.Type == FlowNodeType.Start).ToList();
        if (startNodes.Count == 0)
            errors.Add("Workflow must have exactly 1 start node; found 0.");
        else if (startNodes.Count > 1)
            errors.Add($"Workflow must have exactly 1 start node; found {startNodes.Count}.");

        // At least 1 end node
        var endNodes = states.Where(s => s.Type == FlowNodeType.End).ToList();
        if (endNodes.Count == 0)
            errors.Add("Workflow must have at least 1 end node.");

        // Connectivity: every non-end node must have at least 1 outgoing transition
        var stateIds = states.Select(s => s.Id).ToHashSet();
        var endNodeIds = endNodes.Select(s => s.Id).ToHashSet();
        var startNodeIds = startNodes.Select(s => s.Id).ToHashSet();

        var outgoing = new Dictionary<Guid, List<Guid>>();
        var incoming = new Dictionary<Guid, int>();
        foreach (var sid in stateIds)
        {
            outgoing[sid] = [];
            incoming[sid] = 0;
        }

        foreach (var t in transitions)
        {
            if (outgoing.ContainsKey(t.FromStateId))
                outgoing[t.FromStateId].Add(t.ToStateId);
            if (incoming.ContainsKey(t.ToStateId))
                incoming[t.ToStateId]++;
        }

        foreach (var state in states)
        {
            if (!endNodeIds.Contains(state.Id) && outgoing[state.Id].Count == 0)
                errors.Add($"Node '{state.Name}' (non-end) has no outgoing transitions.");

            if (!startNodeIds.Contains(state.Id) && incoming[state.Id] == 0)
                errors.Add($"Node '{state.Name}' (non-start) has no incoming transitions.");
        }

        // Cycle detection: remove end nodes, then check for cycles in remaining graph
        var nonEndIds = states.Where(s => s.Type != FlowNodeType.End).Select(s => s.Id).ToHashSet();
        if (nonEndIds.Count > 0)
        {
            var adjReduced = new Dictionary<Guid, List<Guid>>();
            foreach (var sid in nonEndIds)
                adjReduced[sid] = [];

            foreach (var t in transitions)
            {
                if (nonEndIds.Contains(t.FromStateId) && nonEndIds.Contains(t.ToStateId))
                    adjReduced[t.FromStateId].Add(t.ToStateId);
            }

            // DFS-based cycle detection
            var white = new HashSet<Guid>(nonEndIds); // unvisited
            var gray = new HashSet<Guid>();            // in current path
            bool hasCycle = false;

            void Dfs(Guid node)
            {
                if (hasCycle) return;
                white.Remove(node);
                gray.Add(node);

                foreach (var neighbor in adjReduced[node])
                {
                    if (gray.Contains(neighbor))
                    {
                        hasCycle = true;
                        return;
                    }
                    if (white.Contains(neighbor))
                        Dfs(neighbor);
                }

                gray.Remove(node);
            }

            while (white.Count > 0 && !hasCycle)
            {
                Dfs(white.First());
            }

            if (hasCycle)
                errors.Add("Workflow contains a cycle that does not pass through an end node.");
        }

        // Validate service_call nodes: external_services exist and are active
        var serviceCallStates = states.Where(s => s.Type == FlowNodeType.ServiceCall).ToList();
        foreach (var sc in serviceCallStates)
        {
            var serviceIdCfg = sc.Configs.FirstOrDefault(c => c.Key == "service_id");
            if (serviceIdCfg is not null && Guid.TryParse(serviceIdCfg.Value, out var serviceId))
            {
                var service = await externalServiceRepository.GetByIdAsync(serviceId, ct);
                if (service is null)
                    errors.Add($"Node '{sc.Name}': external service {serviceId} not found.");
                else if (!service.IsActive)
                    errors.Add($"Node '{sc.Name}': external service '{service.Name}' is not active.");
            }
        }

        // Validate send_message nodes: notification_templates exist
        var sendMessageStates = states.Where(s => s.Type == FlowNodeType.SendMessage).ToList();
        foreach (var sm in sendMessageStates)
        {
            var templateIdCfg = sm.Configs.FirstOrDefault(c => c.Key == "template_id");
            if (templateIdCfg is not null && Guid.TryParse(templateIdCfg.Value, out var templateId))
            {
                var template = await notificationTemplateRepository.GetByIdAsync(templateId, ct);
                if (template is null)
                    errors.Add($"Node '{sm.Name}': notification template {templateId} not found.");
            }
        }

        // Validate data_capture nodes have at least 1 field
        var dataCaptureStates = states.Where(s => s.Type == FlowNodeType.DataCapture).ToList();
        foreach (var dc in dataCaptureStates)
        {
            if (dc.Fields.Count == 0)
                errors.Add($"Node '{dc.Name}': data_capture must have at least 1 field.");
        }

        // If errors, throw with all of them
        if (errors.Count > 0)
            throw new WorkflowValidationException(errors);

        // Activate
        workflow.Activate(command.UpdatedBy);
        workflowRepository.Update(workflow);
        await workflowRepository.SaveChangesAsync(ct);

        // Publish event
        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: workflow.TenantId,
            UserId: command.UpdatedBy,
            UserName: string.Empty,
            Operation: "WRITE",
            Module: "config",
            Action: "workflow_published",
            Detail: $"Workflow '{workflow.Name}' activated (v{workflow.Version})",
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

using Mediator;
using SA.Config.Application.Dtos.Workflows;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Queries.Workflows;

public sealed class GetWorkflowDetailQueryHandler(
    IWorkflowRepository workflowRepository) : IQueryHandler<GetWorkflowDetailQuery, WorkflowDetailDto>
{
    public async ValueTask<WorkflowDetailDto> Handle(GetWorkflowDetailQuery query, CancellationToken ct)
    {
        var workflow = await workflowRepository.GetByIdFullAsync(query.Id, ct)
            ?? throw new NotFoundException("WORKFLOW_NOT_FOUND", "Workflow no encontrado.");

        return MapToDetailDto(workflow);
    }

    internal static WorkflowDetailDto MapToDetailDto(Domain.Entities.WorkflowDefinition workflow)
    {
        var states = workflow.States.Select(s => new WorkflowStateDto(
            s.Id,
            s.Name,
            s.Label,
            s.Type.ToString(),
            s.PositionX,
            s.PositionY,
            s.Configs.Select(c => new WorkflowStateConfigDto(c.Key, c.Value)).ToList(),
            s.Fields.Select(f => new WorkflowStateFieldDto(f.FieldName, f.FieldType, f.IsRequired, f.SortOrder)).ToList()))
            .ToList();

        var transitions = workflow.Transitions.Select(t => new WorkflowTransitionDto(
            t.Id,
            t.FromStateId,
            t.ToStateId,
            t.Label,
            t.Condition,
            t.SlaHours))
            .ToList();

        return new WorkflowDetailDto(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Status.ToString(),
            workflow.Version,
            workflow.CreatedAt,
            workflow.UpdatedAt,
            workflow.CreatedBy,
            workflow.UpdatedBy,
            states,
            transitions);
    }
}

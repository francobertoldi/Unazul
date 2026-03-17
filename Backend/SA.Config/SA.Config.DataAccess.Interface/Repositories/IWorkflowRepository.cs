using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;

namespace SA.Config.DataAccess.Interface.Repositories;

public interface IWorkflowRepository
{
    Task<(IReadOnlyList<WorkflowDefinition> Items, int Total)> ListAsync(int skip, int take, string? search = null, WorkflowStatus? status = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
    Task<WorkflowDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkflowDefinition?> GetByIdFullAsync(Guid id, CancellationToken ct = default); // Includes states + configs + fields + transitions
    Task AddAsync(WorkflowDefinition workflow, CancellationToken ct = default);
    void Update(WorkflowDefinition workflow);
    Task ReplaceChildrenAsync(Guid workflowId, Guid tenantId, IEnumerable<WorkflowState> states, IEnumerable<WorkflowTransition> transitions, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

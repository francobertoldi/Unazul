using Microsoft.EntityFrameworkCore;
using SA.Config.DataAccess.EntityFramework.Persistence;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;

namespace SA.Config.DataAccess.EntityFramework.Repositories;

public sealed class WorkflowRepository(ConfigDbContext db) : IWorkflowRepository
{
    public async Task<(IReadOnlyList<WorkflowDefinition> Items, int Total)> ListAsync(
        int skip,
        int take,
        string? search = null,
        WorkflowStatus? status = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default)
    {
        var query = db.WorkflowDefinitions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(w =>
                EF.Functions.ILike(w.Name, $"%{search}%") ||
                EF.Functions.ILike(w.Description ?? "", $"%{search}%"));
        }

        if (status.HasValue)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        var total = await query.CountAsync(ct);

        var isDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLower() switch
        {
            "status" => isDescending ? query.OrderByDescending(w => w.Status) : query.OrderBy(w => w.Status),
            "created_at" => isDescending ? query.OrderByDescending(w => w.CreatedAt) : query.OrderBy(w => w.CreatedAt),
            "updated_at" => isDescending ? query.OrderByDescending(w => w.UpdatedAt) : query.OrderBy(w => w.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(w => w.Name) : query.OrderBy(w => w.Name),
        };

        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<WorkflowDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.WorkflowDefinitions.FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<WorkflowDefinition?> GetByIdFullAsync(Guid id, CancellationToken ct = default)
    {
        return await db.WorkflowDefinitions.AsNoTracking()
            .Include(w => w.States).ThenInclude(s => s.Configs)
            .Include(w => w.States).ThenInclude(s => s.Fields.OrderBy(f => f.SortOrder))
            .Include(w => w.Transitions)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task AddAsync(WorkflowDefinition workflow, CancellationToken ct = default)
    {
        await db.WorkflowDefinitions.AddAsync(workflow, ct);
    }

    public void Update(WorkflowDefinition workflow)
    {
        db.WorkflowDefinitions.Update(workflow);
    }

    public async Task ReplaceChildrenAsync(
        Guid workflowId,
        Guid tenantId,
        IEnumerable<WorkflowState> states,
        IEnumerable<WorkflowTransition> transitions,
        CancellationToken ct = default)
    {
        // Remove existing transitions first (FK references states).
        var existingTransitions = await db.WorkflowTransitions
            .Where(t => t.WorkflowId == workflowId)
            .ToListAsync(ct);
        db.WorkflowTransitions.RemoveRange(existingTransitions);

        // Remove existing state children (configs + fields cascade), then states.
        var existingStates = await db.WorkflowStates
            .Where(s => s.WorkflowId == workflowId)
            .ToListAsync(ct);
        db.WorkflowStates.RemoveRange(existingStates);

        // Add new states (with configs + fields attached) and transitions.
        await db.WorkflowStates.AddRangeAsync(states, ct);
        await db.WorkflowTransitions.AddRangeAsync(transitions, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

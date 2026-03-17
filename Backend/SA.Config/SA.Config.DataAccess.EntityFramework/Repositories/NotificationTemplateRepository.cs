using Microsoft.EntityFrameworkCore;
using SA.Config.DataAccess.EntityFramework.Persistence;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;

namespace SA.Config.DataAccess.EntityFramework.Repositories;

public sealed class NotificationTemplateRepository(ConfigDbContext db) : INotificationTemplateRepository
{
    public async Task<(IReadOnlyList<NotificationTemplate> Items, int Total)> ListAsync(
        int skip,
        int take,
        string? channel = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var query = db.NotificationTemplates.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = query.Where(t => t.Channel == channel);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                EF.Functions.ILike(t.Name, $"%{search}%") ||
                EF.Functions.ILike(t.Code, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.NotificationTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        return await db.NotificationTemplates
            .AnyAsync(t => t.TenantId == tenantId && t.Code == code, ct);
    }

    public async Task<bool> IsReferencedByActiveWorkflowAsync(Guid templateId, CancellationToken ct = default)
    {
        // A template is referenced when any workflow_state_config has key='template_id'
        // with the template's Id as value, and the parent workflow is active.
        var templateIdStr = templateId.ToString();

        return await db.WorkflowStateConfigs
            .Where(c => c.Key == "template_id" && c.Value == templateIdStr)
            .Join(
                db.WorkflowStates,
                config => config.StateId,
                state => state.Id,
                (config, state) => state.WorkflowId)
            .Join(
                db.WorkflowDefinitions.Where(w => w.Status == WorkflowStatus.Active),
                workflowId => workflowId,
                workflow => workflow.Id,
                (workflowId, workflow) => workflow)
            .AnyAsync(ct);
    }

    public async Task AddAsync(NotificationTemplate template, CancellationToken ct = default)
    {
        await db.NotificationTemplates.AddAsync(template, ct);
    }

    public void Update(NotificationTemplate template)
    {
        db.NotificationTemplates.Update(template);
    }

    public Task DeleteAsync(NotificationTemplate template, CancellationToken ct = default)
    {
        db.NotificationTemplates.Remove(template);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}

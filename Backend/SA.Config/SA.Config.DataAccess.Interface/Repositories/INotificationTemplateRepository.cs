using SA.Config.Domain.Entities;

namespace SA.Config.DataAccess.Interface.Repositories;

public interface INotificationTemplateRepository
{
    Task<(IReadOnlyList<NotificationTemplate> Items, int Total)> ListAsync(int skip, int take, string? channel = null, string? search = null, CancellationToken ct = default);
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<bool> IsReferencedByActiveWorkflowAsync(Guid templateId, CancellationToken ct = default);
    Task AddAsync(NotificationTemplate template, CancellationToken ct = default);
    void Update(NotificationTemplate template);
    Task DeleteAsync(NotificationTemplate template, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

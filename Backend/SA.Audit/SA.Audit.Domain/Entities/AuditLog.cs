namespace SA.Audit.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string Operation { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? Detail { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? ChangesJson { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid tenantId,
        Guid userId,
        string userName,
        string operation,
        string module,
        string action,
        string? detail,
        string ipAddress,
        string? entityType,
        Guid? entityId,
        string? changesJson,
        DateTimeOffset occurredAt)
    {
        return new AuditLog
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            UserId = userId,
            UserName = userName,
            Operation = operation,
            Module = module,
            Action = action,
            Detail = detail,
            IpAddress = ipAddress,
            EntityType = entityType,
            EntityId = entityId,
            ChangesJson = changesJson,
            OccurredAt = occurredAt
        };
    }
}

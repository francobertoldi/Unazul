namespace SA.Operations.Domain.Entities;

public sealed class TraceEvent
{
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid TenantId { get; private set; }
    public string State { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }

    public ICollection<TraceEventDetail> Details { get; private set; } = [];

    private TraceEvent() { }

    public static TraceEvent Create(
        Guid applicationId,
        Guid tenantId,
        string state,
        string action,
        Guid userId,
        string userName)
    {
        return new TraceEvent
        {
            Id = Guid.CreateVersion7(),
            ApplicationId = applicationId,
            TenantId = tenantId,
            State = state,
            Action = action,
            UserId = userId,
            UserName = userName,
            OccurredAt = DateTime.UtcNow
        };
    }
}

namespace SA.Operations.Application.Events;

public sealed record ApplicationStatusChangedEvent(
    Guid ApplicationId,
    string OldStatus,
    string NewStatus,
    Guid UserId,
    Guid TenantId,
    DateTimeOffset OccurredAt,
    Guid CorrelationId);
